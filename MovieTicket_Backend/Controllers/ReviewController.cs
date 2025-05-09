using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Repositories;
using MovieTicket_Backend.RepositoryImpl;
using MovieTicket_Backend.RepositoryInpl;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/v1/review")]
    [ApiController]
    public class ReviewController : Controller
    {
        private readonly ILogger<MovieController> _logger;
        private readonly IConfiguration _configuration;
        private readonly IReviewRepository _reviewRepository;
        private readonly MovieRepository _movieRepository;

        public ReviewController(ILogger<MovieController> logger, IConfiguration configuration, IReviewRepository reviewRepository, MovieRepository movieRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _reviewRepository = reviewRepository;
            _movieRepository = movieRepository;
        }

        [HttpGet("{movieId}")]
        public async Task<IActionResult> GetReviewsByMovieId(int movieId)
        {
            var movie = await _movieRepository.GetMovieById(movieId);
            if (movie == null)
            {
                return NotFound(new { status = "error", message = "Movie not found" });
            }
            var reviews = await _reviewRepository.GetReviewsByMovieId(movieId);
            return Ok(reviews);
        }
    }
}
