using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Models.ModelDTOs;
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

        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var result = await _reviewRepository.DeleteReview(reviewId);
            if (!result)
            {
                return NotFound(new { status = "error", message = "Review not found" });
            }
            return Ok(new { status = "success", message = "Review deleted successfully" });
        }

        [HttpPost("add")]
        public async Task<IActionResult> CreateReview([FromBody] CreateReviewDTO review)
        {
            if (review == null)
            {
                return BadRequest(new { status = "error", message = "Invalid review data" });
            }
            var result = await _reviewRepository.CreateReview(review);
            if (!result)
            {
                return BadRequest(new { status = "error", message = "Failed to create review" });
            }
            return Ok(new { status = "success", message = "Review created successfully" });
        }

        [HttpPatch("{reviewId}")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDTO updateReview)
        {
            if (string.IsNullOrWhiteSpace(updateReview.NewContent))
            {
                return BadRequest(new { status = "error", message = "Invalid review data" });
            }
            var result = await _reviewRepository.UpdateReview(reviewId, updateReview.NewContent, updateReview.NewRating);
            if (!result)
            {
                return NotFound(new { status = "error", message = "Review not found" });
            }
            return Ok(new { status = "success", message = "Review updated successfully" });
        }

        [HttpPost("{reviewId}/like")]
        public async Task<IActionResult> UpdateReviewLike(int reviewId)
        {
            var result = await _reviewRepository.UpdateReviewLike(reviewId);
            if (!result)
            {
                return NotFound(new { status = "error", message = "Review not found" });
            }
            return Ok(new { status = "success", message = "Review liked successfully" });
        }

        [HttpPost("{reviewId}/unlike")]
        public async Task<IActionResult> UpdateReviewUnlike(int reviewId)
        {
            var result = await _reviewRepository.UpdateReviewUnlike(reviewId);
            if (!result)
            {
                return NotFound(new { status = "error", message = "Review not found" });
            }
            return Ok(new { status = "success", message = "Review unliked successfully" });
        }
    }
}
