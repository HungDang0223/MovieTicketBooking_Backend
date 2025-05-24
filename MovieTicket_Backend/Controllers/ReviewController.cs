using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.ModelDTOs;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Repositories;
using MovieTicket_Backend.RepositoryImpl;
using MovieTicket_Backend.RepositoryInpl;
using System.Security.Claims;

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
        private readonly IAuthenticationRepository _authenticationRepository;

        public ReviewController(ILogger<MovieController> logger, IConfiguration configuration, IReviewRepository reviewRepository, MovieRepository movieRepository, IAuthenticationRepository authenticationRepository)
        {
            _logger = logger;
            _configuration = configuration;
            _reviewRepository = reviewRepository;
            _movieRepository = movieRepository;
            _authenticationRepository = authenticationRepository;
        }

        // query by page and pageSize
        
        [HttpGet("{movieId}")]
        public async Task<IActionResult> GetReviewsByMovieId(int movieId, int page, int limit, string? sort)
        {
            var movie = await _movieRepository.GetMovieById(movieId);
            if (movie == null)
            {
                return NotFound(new { status = "error", message = "Movie not found" });
            }
            var reviews = await _reviewRepository.GetReviewsByMovieId(movieId, page, limit, sort);
            return Ok(new { status = "success", message = "Lấy danh sách review thành công", data = reviews});
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

        [HttpPost("{movieId}")]
        public async Task<IActionResult> CreateReview(int movieId, [FromBody] CreateReviewDTO review)
        {
            if (review == null)
            {
                return BadRequest(new { status = "error", message = "Invalid review data" });
            }
            var result = await _reviewRepository.CreateReview(movieId, review);
            if (result == null)
            {
                return BadRequest(new { status = "error", message = "Failed to create review" });
            }
            return Ok(new { status = "success", message = "Review created successfully", data = result });
        }

        [HttpPatch("{reviewId}")]
        public async Task<IActionResult> UpdateReview(int reviewId, [FromBody] UpdateReviewDTO updateReview)
        {
            if (string.IsNullOrWhiteSpace(updateReview.NewContent))
            {
                return BadRequest(new { status = "error", message = "Invalid review data" });
            }
            var result = await _reviewRepository.UpdateReview(reviewId, updateReview.NewContent, updateReview.NewRating);
            if (result == null)
            {
                return BadRequest(new { status = "error", message = "Failed to update review" });
            }
            return Ok(new { status = "success", message = "Review created successfully", data = result });
        }

        [HttpPatch("{reviewId}/like")]
        public async Task<IActionResult> UpdateReviewLike(int reviewId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            // Lấy userId từ token
            var userId = _authenticationRepository.GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized(new { status = "error", message = "Unauthorized" });
            }
            var result = await _reviewRepository.UpdateReviewLike(reviewId, userId);
            if (!result)
            {
                return NotFound(new { status = "error", message = "Review not found or Update failed" });
            }
            return Ok(new { status = "success", message = "Review liked successfully" });
        }

        [HttpPatch("{reviewId}/unlike")]
        public async Task<IActionResult> UpdateReviewUnlike(int reviewId)
        {
            var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", string.Empty);
            // Lấy userId từ token
            var userId = _authenticationRepository.GetUserIdFromToken(token);
            if (userId == null)
            {
                return Unauthorized(new { status = "error", message = "Unauthorized" });
            }
            var result = await _reviewRepository.UpdateReviewUnlike(reviewId, userId);
            if (!result)
            {
                return NotFound(new { status = "error", message = "Review not found or Update failed" });
            }
            return Ok(new { status = "success", message = "Review unliked successfully" });
        }
    }
}
