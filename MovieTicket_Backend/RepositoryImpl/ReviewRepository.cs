using Dapper;
using Microsoft.EntityFrameworkCore;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Models.ModelDTOs;
using MovieTicket_Backend.Repositories;

namespace MovieTicket_Backend.RepositoryImpl
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly ApplicationDbContext _context;
        public ReviewRepository(DbConnectionFactory dbConnectionFactory, ApplicationDbContext context)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _context = context;
        }

        public Task<bool> CreateReview(CreateReviewDTO review)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var query = @"INSERT INTO reviews (user_id, movie_id, rating, review_content, review_date) 
                            VALUES (@UserId, @MovieId, @Rating, @ReviewContent, @ReviewDate)";
            var rowEffect = connection.Execute(query, new
            {
                UserId = review.UserId,
                MovieId = review.MovieId,
                Rating = review.Rating,
                ReviewContent = review.ReviewContent,
                ReviewDate = review.ReviewDate
            });
            if (rowEffect > 0)
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        public Task<bool> DeleteReview(int reviewId)
        {
            var rowEffect = _context.Reviews.Remove(new Review { ReviewId = reviewId });
            _context.SaveChanges();
            if (rowEffect != null)
            {
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        public async Task<List<Review>> GetReviewsByMovieId(int movieId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var reviews = await _context.Reviews
                .Where(r => r.MovieId == movieId)
                .Join(_context.Users,
                    review => review.UserId,
                    user => user.UserId,
                    (review, user) => new Review
                    {
                        ReviewId = review.ReviewId,
                        UserId = review.UserId,
                        FullName = user.FullName,
                        PhotoPath = user.PhotoUrl,
                        MovieId = review.MovieId,
                        Rating = review.Rating,
                        ReviewContent = review.ReviewContent,
                        ReviewDate = review.ReviewDate,
                        Likes = review.Likes,
                        Unlikes = review.Unlikes
                    })
                .ToListAsync();
            return reviews;
        }
        public Task<bool> UpdateReview(int reviewId, string newContent, int newRating)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review != null)
            {
                review.ReviewContent = newContent;
                review.Rating = newRating;
                _context.SaveChanges();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        public Task<bool> UpdateReviewLike(int reviewId)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review != null)
            {
                review.Likes += 1;
                _context.SaveChanges();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
        public Task<bool> UpdateReviewUnlike(int reviewId)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review != null)
            {
                review.Unlikes += 1;
                _context.SaveChanges();
                return Task.FromResult(true);
            }
            return Task.FromResult(false);
        }
    }
}
