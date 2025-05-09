using Dapper;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Repositories;

namespace MovieTicket_Backend.RepositoryImpl
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        public ReviewRepository(DbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public Task<bool> CreateReview(Review review)
        {
            throw new NotImplementedException();
        }
        public Task<bool> DeleteReview(int reviewId)
        {
            throw new NotImplementedException();
        }
        public async Task<List<Review>> GetReviewsByMovieId(int movieId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var query = "SELECT * FROM review WHERE movie_id = @MovieId";
            var reviews = await connection.QueryAsync<Review>(query, new { MovieId = movieId });
            return reviews.ToList();
        }
        public Task<bool> UpdateReview(Review review)
        {
            throw new NotImplementedException();
        }
    }
}
