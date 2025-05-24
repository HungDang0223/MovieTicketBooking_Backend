using MovieTicket_Backend.ModelDTOs;
using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.Repositories
{
    public interface IReviewRepository
    {
        Task<Review> CreateReview(int movieId, CreateReviewDTO review);
        Task<List<ReviewDTO>> GetReviewsByMovieId(int movieId, int page, int pageSize, string? orderBy);
        Task<bool> DeleteReview(int reviewId);
        Task<Review> UpdateReview(int reviewId, string newContent, int newRating);
        Task<bool> UpdateReviewLike(int reviewId, string currentUserId);
        Task<bool> UpdateReviewUnlike(int reviewId, string currentUserId);
    }
}
