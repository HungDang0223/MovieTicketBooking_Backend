using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.RepositoryInpl;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/v1/movie")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly ILogger<MovieController> _logger;
        private readonly IConfiguration _configuration;
        private readonly MovieRepository _movieRepository;
        public MovieController(ILogger<MovieController> logger, IConfiguration configuration, MovieRepository movieRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _movieRepository = movieRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMovies()
        {
            var movies = await _movieRepository.GetAllMovies();
            if (movies == null || movies.Count == 0)
            {
                return NotFound(new { status = "error", message = "No movies found" });
            }
            return Ok(movies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMovieById(int id, [FromQuery] string? userId=null)
        {
            var movie = await _movieRepository.GetMovieById(id, userId);
            if (movie == null)
            {
                return NotFound(new { status = "error", message = "Movie not found" });
            }
            return Ok(movie);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddMovie([FromBody] Movie movie)
        {
            if (movie == null)
            {
                return BadRequest(new { status = "error", message = "Invalid movie data" });
            }
            var result = await _movieRepository.AddMovie(movie);
            if (!result)
            {
                return BadRequest(new { status = "error", message = "Failed to add movie" });
            }
            return Ok(new { status = "success", message = "Movie added successfully" });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateMovie([FromBody] Movie movie)
        {
            if (movie == null)
            {
                return BadRequest(new { status = "error", message = "Invalid movie data" });
            }
            var result = await _movieRepository.UpdateMovieAsync(movie);
            if (!result)
            {
                return NotFound(new { status = "error", message = "Movie not found or failed to update" });
            }
            return Ok(new { status = "success", message = "Movie updated successfully" });
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteMovie(int id)
        {
            var result = await _movieRepository.DeleteMovie(id);
            if (result == -1)
            {
                return NotFound(new { status = "error", message = "Movie not found" });
            }
            return Ok(new { status = "success", message = "Movie deleted successfully" });
        }
    }
}
