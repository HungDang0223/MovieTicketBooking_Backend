using Dapper;
using Microsoft.EntityFrameworkCore;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Models.ModelDTOs;
using MovieTicket_Backend.Services;

namespace MovieTicket_Backend.RepositoryInpl
{
    public class ShowingMovieRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly ApplicationDbContext _dbContext;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ShowingMovieRepository> _logger;
        public ShowingMovieRepository(DbConnectionFactory dbConnectionFactory, IConfiguration configuration, ApplicationDbContext context, ILogger<ShowingMovieRepository> logger)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _dbContext = context;
            _configuration = configuration;
            _logger = logger;
        }

        /// <summary>
        /// Get all movie showings by movie id and date and cinema id
        /// </summary>
        /// <param name="movieId"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        public async Task<List<Dictionary<string, dynamic>>> GetGroupedShowings(int movieId, DateOnly date)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                SELECT 
                    ms.showing_id AS ShowingId,
                    ms.screen_id AS ScreenId,
                    ms.start_time AS StartTime,
                    ms.end_time AS EndTime,
                    ms.showing_date AS ShowingDate,
                    ms.showing_format AS ShowingFormat,
                    c.cinema_name AS CinemaName,
                    s.screen_name AS ScreenName,
                    SUM(sr.seat_count) AS SeatCount
                FROM showing_movie ms
                    JOIN movie m ON ms.movie_id = m.movie_id
                    JOIN screen s ON ms.screen_id = s.screen_id
                    JOIN cinema c ON s.cinema_id = c.cinema_id
                    JOIN screen_row sr ON sr.screen_id = s.screen_id
                WHERE ms.movie_id = @MovieId 
                  AND ms.showing_date = @Date
                GROUP BY ms.showing_id, ms.screen_id, ms.start_time, ms.end_time, ms.showing_date, ms.showing_format, c.cinema_name, s.screen_name";
            var parameters = new
            {
                MovieId = movieId,
                Date = date.ToDateTime(TimeOnly.MinValue)
            };

            var showings = await connection.QueryAsync<ShowingMovieDTO>(query, parameters);

            // return
            // name: CinemaName
            // showing: List<ShowingMovieDTO>

            var result = new List<Dictionary<string, dynamic>>();
            foreach (var showing in showings) {
                var cinemaName = showing.CinemaName;
                var existingCinema = result.FirstOrDefault(x => x["CinemaName"].ToString() == cinemaName);
                if (existingCinema == null)
                {
                    existingCinema = new Dictionary<string, dynamic>
                    {
                        { "CinemaName", cinemaName },
                        { "Showings", new List<ShowingMovieDTO>() }
                    };
                    result.Add(existingCinema);
                }
                ((List<ShowingMovieDTO>)existingCinema["Showings"]).Add(showing);
            }

            return result;
        }

        public async Task<List<ShowingMovieDTO>> GetShowingsMovieByCinemaId(int cinemaId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                SELECT 
                    ms.showing_id AS ShowingId,
                    ms.screen_id AS ScreenId,
                    ms.start_time AS StartTime,
                    ms.end_time AS EndTime,
                    ms.showing_date AS ShowingDate,
                    ms.showing_format AS ShowingFormat,
                    c.cinema_name AS CinemaName,
                    s.screen_name AS ScreenName,
                    SUM(sr.seat_count) AS SeatCount
                FROM showing_movie ms
                    JOIN movie m ON ms.movie_id = m.movie_id
                    JOIN screen s ON ms.screen_id = s.screen_id
                    JOIN cinema c ON s.cinema_id = c.cinema_id
                    JOIN screen_row sr ON sr.screen_id = s.screen_id
                WHERE c.cinema_id = @CinemaId
                GROUP BY ms.start_time, ms.end_time, ms.showing_date, ms.showing_format, c.cinema_name, s.screen_name";
            var parameters = new { CinemaId = cinemaId };
            var showings = await connection.QueryAsync<ShowingMovieDTO>(query, parameters);
            return showings.ToList();
        }

        // Fix for CS0019: Operator '==' cannot be applied to operands of type 'IEnumerable<int>' and 'int'  
        // Updated the LINQ query to use `.Contains()` for comparison instead of `==`.  

        public async Task<int> InsertBatchShowingSeat(int showingId)
        {
            // Lấy showing movie  
            using var connection = _dbConnectionFactory.CreateConnection();
            var screenIdQuery = @"SELECT screen_id FROM showing_movie WHERE showing_id = @ShowingId";
            var creenId = await connection.QueryFirstAsync<int>(screenIdQuery, new { ShowingId = showingId });

            // Lấy tất cả các hàng ghế trong screen  
            var rows = await _dbContext.ScreenRows
                .Where(sr => sr.ScreenId == creenId)
                .ToListAsync();
            _logger.LogInformation("Rows: {rows}", rows);

            // Lấy tất cả các ghế trong các hàng ghế ở trên  
            var rowIds = rows.Select(r => r.RowId).ToList();
            var seats = await _dbContext.Seats
                .Where(s => rowIds.Contains(s.RowId))
                .ToListAsync();
            _logger.LogInformation("Seats: {seats}", seats);

            if (seats != null)
            {
                string query = @"  
                   INSERT INTO showing_seat (showing_id, seat_id)   
                   VALUES (@ShowingId, @SeatId)";
                foreach (var seat in seats)
                {
                    var parameters = new { ShowingId = showingId, SeatId = seat.SeatId };
                    await connection.ExecuteAsync(query, parameters);
                }
                return seats.Count;
            }
            return 0;
        }
    }
}
