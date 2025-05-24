using Dapper;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.RepositoryInpl;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MovieTicket_Backend.Services
{
    public class BatchMovieShowingService
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly IConfiguration _configuration;
        private readonly ShowingMovieRepository _showingMovieRepository;

        public BatchMovieShowingService(DbConnectionFactory dbConnectionFactory, IConfiguration configuration, ShowingMovieRepository showingMovieRepository)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _configuration = configuration;
            _showingMovieRepository = showingMovieRepository;
        }

        /// <summary>
        /// Tạo nhiều suất chiếu cho một phim tại một rạp trong một khoảng thời gian
        /// </summary>
        /// <param name="request">Thông tin về các suất chiếu cần tạo</param>
        /// <returns>Danh sách các suất chiếu đã được tạo</returns>
        public async Task<List<ShowingMovie>> CreateBatchShowings(BatchShowingRequest request)
        {
            // Lấy danh sách các màn hình của rạp
            var screens = await GetScreensByCinemaId(request.CinemaId);
            if (screens.Count == 0)
            {
                throw new Exception($"Không tìm thấy màn hình nào cho rạp có ID {request.CinemaId}");
            }

            // Lấy thông tin phim để biết thời lượng
            var movie = await GetMovieById(request.MovieId);
            if (movie == null)
            {
                throw new Exception($"Không tìm thấy phim có ID {request.MovieId}");
            }

            // Lấy các suất chiếu hiện có của rạp trong ngày đã chọn
            var existingShowings = await GetExistingShowings(request.CinemaId, request.ShowingDate);

            // Tạo các suất chiếu mới
            var newShowings = new List<ShowingMovie>();
            var startTime = request.StartTime;
            var endTime = request.EndTime;

            // Thêm đủ số lượng suất chiếu hoặc đến khi không còn thời gian phù hợp
            for (int i = 0; i < request.NumberOfShowings; i++)
            {
                // Tìm một màn hình có sẵn cho suất chiếu này
                var (availableScreen, proposedStartTime, proposedEndTime) = FindAvailableScreen(
                    screens,
                    existingShowings,
                    startTime,
                    endTime,
                    movie.Duration,
                    request.ShowingDate,
                    request.GapBetweenShowings,
                    request.UseConsecutiveScreens
                );

                if (availableScreen == null)
                {
                    // Không còn màn hình trống phù hợp
                    break;
                }

                // Tạo suất chiếu mới
                var newShowing = new ShowingMovie
                {
                    MovieId = request.MovieId,
                    ScreenId = availableScreen.ScreenId,
                    StartTime = proposedStartTime.ToTimeSpan(),
                    EndTime = proposedEndTime.ToTimeSpan(),
                    ShowingDate = request.ShowingDate.ToDateTime(TimeOnly.MinValue),
                    ShowingFormat = request.ShowingFormat
                };

                // Thêm vào cơ sở dữ liệu
                var insertedShowing = await InsertShowingMovie(newShowing);
                newShowings.Add(insertedShowing);

                // Thêm vào danh sách suất chiếu hiện tại để tránh trùng lặp trong lần lặp tiếp theo
                existingShowings.Add(new ExistingShowingInfo
                {
                    ScreenId = availableScreen.ScreenId,
                    StartTime = proposedStartTime.ToTimeSpan(),
                    EndTime = proposedEndTime.ToTimeSpan(),
                    ShowingDate = request.ShowingDate.ToDateTime(TimeOnly.MinValue)
                });

                // Cập nhật giờ bắt đầu cho lần lặp tiếp theo
                startTime = proposedEndTime.AddMinutes(request.GapBetweenShowings);
            }

            return newShowings;
        }

        /// <summary>
        /// Tìm một màn hình có sẵn cho suất chiếu
        /// </summary>
        /// Dùng đệ quy để tìm và insert suất chiếu hàng loạt
        private (ScreenInfo? availableScreen, TimeOnly proposedStartTime, TimeOnly proposedEndTime) FindAvailableScreen(
            List<ScreenInfo> screens,
            List<ExistingShowingInfo> existingShowings,
            TimeOnly preferredStartTime, // Thời điểm đề xuất
            TimeOnly endTime,
            int movieDuration,
            DateOnly showingDate,
            int gapBetweenShowings,
            bool useConsecutiveScreens = true,
            int recursionDepth = 0)  // Giới hạn số lần lặp đệ quy
        {
            // Dừng lại ở 11
            if (recursionDepth > 10)
            {
                return (null, preferredStartTime, preferredStartTime.AddMinutes(movieDuration));
            }

            var proposedEndTime = preferredStartTime.AddMinutes(movieDuration);

            // Kiểm tra nếu thời gian đề xuất vượt quá giới hạn
            if (proposedEndTime > endTime)
            {
                return (null, preferredStartTime, proposedEndTime);
            }

            // Lọc các màn hình đang có sẵn vào thời điểm đề xuất
            foreach (var screen in screens)
            {
                var isAvailable = true;
                var screenShowings = existingShowings.Where(s => s.ScreenId == screen.ScreenId).ToList();

                foreach (var showing in screenShowings)
                {
                    var existingStart = RoundingTime(TimeOnly.FromTimeSpan(showing.StartTime));
                    var existingEnd = RoundingTime(TimeOnly.FromTimeSpan(showing.EndTime));

                    // Kiểm tra xem có bị chồng lấp không
                    // TH 1: Suất mới bắt đầu trong khi suất cũ đang chiếu
                    // TH 2: Suất mới kết thúc trong khi suất cũ đang chiếu
                    // TH 3: Suất mới chồng lên toàn bộ suất cũ
                    if ((preferredStartTime >= existingStart && preferredStartTime < existingEnd) ||
                        (proposedEndTime > existingStart && proposedEndTime <= existingEnd) ||
                        (preferredStartTime <= existingStart && proposedEndTime >= existingEnd))
                    {
                        isAvailable = false;

                        // Nếu là cùng một màn hình, thử đặt lịch sau suất chiếu này + thời gian dọn dẹp
                        if (useConsecutiveScreens)
                        {
                            var newProposedStart = existingEnd.AddMinutes(gapBetweenShowings);
                            var newProposedEnd = newProposedStart.AddMinutes(movieDuration);

                            // Kiểm tra xem thời gian mới có nằm trong phạm vi cho phép không
                            if (newProposedEnd <= endTime)
                            {
                                // Đệ quy kiểm tra với thời gian mới
                                return FindAvailableScreen(
                                    screens,
                                    existingShowings,
                                    newProposedStart,
                                    endTime,
                                    movieDuration,
                                    showingDate,
                                    gapBetweenShowings,
                                    useConsecutiveScreens,
                                    recursionDepth + 1);  // Increment recursion depth
                            }
                        }

                        break;
                    }
                }

                if (isAvailable)
                {
                    return (screen, preferredStartTime, proposedEndTime);
                }
            }

            // Không tìm thấy màn hình nào, thử đẩy giờ bắt đầu lên 30 phút và kiểm tra lại
            var nextStartTime = preferredStartTime.AddMinutes(30);
            if (nextStartTime.AddMinutes(movieDuration) <= endTime)
            {
                return FindAvailableScreen(
                    screens,
                    existingShowings,
                    nextStartTime,
                    endTime,
                    movieDuration,
                    showingDate,
                    gapBetweenShowings,
                    useConsecutiveScreens,
                    recursionDepth + 1);  // Increment recursion depth
            }

            return (null, preferredStartTime, proposedEndTime);
        }

        /// <summary>
        /// Lấy danh sách các màn hình của rạp
        /// </summary>
        private async Task<List<ScreenInfo>> GetScreensByCinemaId(int cinemaId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                SELECT 
                    s.screen_id AS ScreenId,
                    s.screen_name AS ScreenName,
                    s.cinema_id AS CinemaId
                FROM screen s
                WHERE s.cinema_id = @CinemaId";
            var parameters = new { CinemaId = cinemaId };
            var screens = await connection.QueryAsync<ScreenInfo>(query, parameters);
            return screens.ToList();
        }

        /// <summary>
        /// Lấy thông tin phim dựa vào ID
        /// </summary>
        private async Task<MovieInfo> GetMovieById(int movieId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                SELECT 
                    m.movie_id AS MovieId,
                    m.title AS Title,
                    m.duration AS Duration
                FROM movie m
                WHERE m.movie_id = @MovieId";
            var parameters = new { MovieId = movieId };
            return await connection.QueryFirstOrDefaultAsync<MovieInfo>(query, parameters);
        }

        /// <summary>
        /// Lấy các suất chiếu hiện có trong ngày tại rạp
        /// </summary>
        private async Task<List<ExistingShowingInfo>> GetExistingShowings(int cinemaId, DateOnly showingDate)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                SELECT 
                    ms.screen_id AS ScreenId,
                    ms.start_time AS StartTime,
                    ms.end_time AS EndTime,
                    ms.showing_date AS ShowingDate
                FROM showing_movie ms
                JOIN screen s ON ms.screen_id = s.screen_id
                JOIN cinema c ON s.cinema_id = c.cinema_id
                WHERE c.cinema_id = @CinemaId 
                  AND ms.showing_date = @ShowingDate";
            var parameters = new
            {
                CinemaId = cinemaId,
                ShowingDate = showingDate.ToDateTime(TimeOnly.MinValue)
            };
            var showings = await connection.QueryAsync<ExistingShowingInfo>(query, parameters);
            return showings.ToList();
        }

        /// <summary>
        /// Thêm một suất chiếu mới vào cơ sở dữ liệu
        /// </summary>
        private async Task<ShowingMovie> InsertShowingMovie(ShowingMovie movie)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                INSERT INTO showing_movie (movie_id, screen_id, start_time, end_time, showing_date, showing_format, language, subtitle_language)
                VALUES (@MovieId, @ScreenId, @StartTime, @EndTime, @ShowingDate, @ShowingFormat, @Language, @SubtileLanguage);
                SELECT LAST_INSERT_ID();";
            var parameters = new
            {
                MovieId = movie.MovieId,
                ScreenId = movie.ScreenId,
                StartTime = movie.StartTime,
                EndTime = movie.EndTime,
                ShowingDate = movie.ShowingDate,
                ShowingFormat = movie.ShowingFormat,
                Language = "English",
                SubtileLanguage = "Vietnamese"
            };
            var id = await connection.ExecuteScalarAsync<int>(query, parameters);
            movie.ShowingId = id;
            // Thêm các ghế vào suất chiếu
            await _showingMovieRepository.InsertBatchShowingSeat(id);
            return movie;
        }

        public TimeOnly RoundingTime(TimeOnly time)
        {
            // Làm tròn thời gian đến 10 phút gần nhất
            int minutes = (int)time.Minute;
            int roundedMinutes = (minutes / 10) * 10;
            return new TimeOnly(time.Hour, roundedMinutes);
        }
    }

    // Class chứa thông tin yêu cầu tạo hàng loạt suất chiếu
    public class BatchShowingRequest
    {
        public int MovieId { get; set; }
        public int CinemaId { get; set; }
        public DateOnly ShowingDate { get; set; }
        public TimeOnly StartTime { get; set; } // Thời gian bắt đầu cho suất chiếu đầu tiên
        public TimeOnly EndTime { get; set; } // Thời gian kết thúc cho suất chiếu cuối cùng
        public int NumberOfShowings { get; set; } // Số lượng suất chiếu cần tạo
        public string ShowingFormat { get; set; } // Định dạng chiếu (2D, 3D, 4DX, IMAX)
        public int GapBetweenShowings { get; set; } = 30; // Khoảng cách tối thiểu giữa các suất chiếu (phút)
        public bool UseConsecutiveScreens { get; set; } = true; // Nếu true, ưu tiên sử dụng cùng một screen cho các suất liên tiếp
    }

    // Class lưu trữ thông tin màn hình
    public class ScreenInfo
    {
        public int ScreenId { get; set; }
        public string ScreenName { get; set; }
        public int CinemaId { get; set; }
    }

    // Class lưu trữ thông tin phim
    public class MovieInfo
    {
        public int MovieId { get; set; }
        public string Title { get; set; }
        public int Duration { get; set; }
    }

    // Class lưu trữ thông tin suất chiếu hiện có
    public class ExistingShowingInfo
    {
        public int ScreenId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime ShowingDate { get; set; }
    }
}