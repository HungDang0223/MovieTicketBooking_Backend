using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Models.ModelRequests;
using MovieTicket_Backend.RepositoryInpl;
using MovieTicket_Backend.Services;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/v1/showing-movie")]
    [ApiController]
    public class ShowingMovieController : Controller
    {
        private readonly ShowingMovieRepository _movieShowingRepository;
        private readonly BatchMovieShowingService _batchShowingService;
        private readonly DbConnectionFactory _dbConnectionFactory;
        public ShowingMovieController(ShowingMovieRepository movieShowingRepository, DbConnectionFactory dbConnectionFactory, BatchMovieShowingService batchShowingMovieService)
        {
            _movieShowingRepository = movieShowingRepository;
            _batchShowingService = batchShowingMovieService;
            _dbConnectionFactory = dbConnectionFactory;
        }

        // ?movie-id={movieId}&date={date}
        [HttpGet()]
        public async Task<IActionResult> GetMovieShowings([FromQuery] int movie, DateOnly date)
        {
            var showings = await _movieShowingRepository.GetGroupedShowings(movie, date);
            if (showings == null || !showings.Any())
            {
                return NotFound(new { status = "error", message = "Không có suất chiếu nào cho ngày này." });
            }
            return Ok(showings);
        }

        [HttpGet("cinema")]
        public async Task<IActionResult> GetShowingsMovieByCinemaId([FromQuery] int cinema)
        {
            if (cinema <= 0)
            {
                return BadRequest(new { status = "error", message = "Invalid cinema ID" });
            }
            var showings = await _movieShowingRepository.GetShowingsMovieByCinemaId(cinema);
            if (showings == null || !showings.Any())
            {
                return NotFound(new { status = "error", message = "No showings found" });
            }
            return Ok(showings);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateMovieShowing([FromBody] BatchShowingRequest request)
        {
            try
            {
                // Validate đầu vào
                if (request.MovieId <= 0)
                {
                    return BadRequest("MovieId không hợp lệ.");
                }
                if (request.ShowingDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    return BadRequest("Ngày chiếu phải từ ngày hiện tại trở đi.");
                }
                // Thực hiện tạo suất chiếu
                var createdShowings = await _batchShowingService.CreateBatchShowings(request);
                // Trả về kết quả
                return Ok(new
                {
                    Success = true,
                    Message = "Tạo suất chiếu thành công.",
                    Data = createdShowings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi khi tạo suất chiếu: {ex.Message}"
                });
            }
        }

        /// <summary>
        /// API để thêm nhiều suất chiếu cho một phim tại một rạp
        /// </summary>
        [HttpPost("batch")]
        //[Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBatchShowings([FromBody] BatchShowingRequest request)
        {
            try
            {
                // Validate đầu vào
                if (request.MovieId <= 0 || request.CinemaId <= 0 || request.NumberOfShowings <= 0)
                {
                    return BadRequest("Thông tin không hợp lệ. MovieId, CinemaId và NumberOfShowings phải lớn hơn 0.");
                }

                if (request.StartTime >= request.EndTime)
                {
                    return BadRequest("Thời gian bắt đầu phải trước thời gian kết thúc.");
                }

                if (request.ShowingDate < DateOnly.FromDateTime(DateTime.Now))
                {
                    return BadRequest("Ngày chiếu phải từ ngày hiện tại trở đi.");
                }

                if (string.IsNullOrEmpty(request.ShowingFormat))
                {
                    return BadRequest("Định dạng chiếu không được để trống.");
                }

                // Thực hiện tạo các suất chiếu
                var createdShowings = await _batchShowingService.CreateBatchShowings(request);

                // Trả về kết quả
                return Ok(new
                {
                    Success = true,
                    Message = $"Đã tạo thành công {createdShowings.Count} suất chiếu.",
                    Data = createdShowings
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Success = false,
                    Message = $"Lỗi khi tạo suất chiếu: {ex.Message}"
                });
            }
        }
        [HttpPost("seats")]
        public async Task<IActionResult> GetSeatsByShowingId([FromBody] int showingId)
        {
            if (showingId <= 0)
            {
                return BadRequest(new { status = "error", message = "Invalid showing ID" });
            }
            var seatInserted = await _movieShowingRepository.InsertBatchShowingSeat(showingId);
            if (seatInserted == 0)
            {
                return NotFound(new { status = "error", message = "No seats inserted" });
            }
            return Ok(new { status = "success", message = $"{seatInserted} seats inserted" });
        }
    }
}
