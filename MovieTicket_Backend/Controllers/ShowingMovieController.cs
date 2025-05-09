using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models.ModelRequests;
using MovieTicket_Backend.RepositoryInpl;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/v1/showing_movie")]
    [ApiController]
    public class ShowingMovieController : Controller
    {
        private readonly MovieShowingRepository _movieShowingRepository;
        private readonly DbConnectionFactory _dbConnectionFactory;
        public ShowingMovieController(MovieShowingRepository movieShowingRepository, DbConnectionFactory dbConnectionFactory)
        {
            _movieShowingRepository = movieShowingRepository;
            _dbConnectionFactory = dbConnectionFactory;
        }

        [HttpGet()]
        public async Task<IActionResult> GetMovieShowings([FromQuery] MovieShowingRequest request)
        {
            var movieId = request.MovieId;
            var date = request.ShowingDate;
            var cinemaBrandId = request.CinemaBrandId;
            if (movieId <= 0 || cinemaBrandId <= 0)
            {
                return (BadRequest(new { status = "error", message = "Invalid movie or cinema ID" }));
            }
            var showings = await _movieShowingRepository.GetGroupedShowings(movieId, date, cinemaBrandId);
            if (showings == null || !showings.Any())
            {
                return NotFound(new { status = "error", message = "No showings found" });
            }
            return Ok(showings);
        }

        //[HttpGet("cinema/{id}")]
        //public async Task<IActionResult> GetShowingsMovieByCinemaId(int id)
        //{
        //    var connection = _dbConnectionFactory.CreateConnection();
        //    var showings = await _movieShowingRepository.GetShowingsMovieByCinemaId(id);
        //    if (showings == null || !showings.Any())
        //    {
        //        return NotFound(new { status = "error", message = "No showings found" });
        //    }
        //    return Ok(new { status = "success", data = showings });
        //}
    }
}
