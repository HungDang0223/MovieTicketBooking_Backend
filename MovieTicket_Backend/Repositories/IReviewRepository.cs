using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.Repositories
{
    public interface IReviewRepository
    {
        Task<bool> CreateReview(Review review);
        Task<List<Review>> GetReviewsByMovieId(int movieId);
        Task<bool> DeleteReview(int reviewId);
        Task<bool> UpdateReview(Review review);
    }
}
