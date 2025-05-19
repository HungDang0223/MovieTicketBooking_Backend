using MovieTicket_Backend.Models;
using MovieTicket_Backend.Models.ModelDTOs;

namespace MovieTicket_Backend.Repositories
{
    public interface IReviewRepository
    {
        Task<bool> CreateReview(CreateReviewDTO review);
        Task<List<Review>> GetReviewsByMovieId(int movieId);
        Task<bool> DeleteReview(int reviewId);
        Task<bool> UpdateReview(int reviewId, string newContent, int newRating);
        Task<bool> UpdateReviewLike(int reviewId);
        Task<bool> UpdateReviewUnlike(int reviewId);
    }
}
