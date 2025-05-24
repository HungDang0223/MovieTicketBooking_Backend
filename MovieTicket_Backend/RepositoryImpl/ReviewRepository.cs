using Dapper;
using Microsoft.EntityFrameworkCore;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.ModelDTOs;
using MovieTicket_Backend.Models;
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

        public Task<Review> CreateReview(int movieId, CreateReviewDTO review)
        {
            var newReview = new Review
            {
                UserId = review.UserId,
                MovieId = movieId,
                Rating = review.Rating,
                ReviewContent = review.ReviewContent,
                ReviewDate = DateTime.Now,
                Likes = 0,
                Unlikes = 0
            };
            _context.Reviews.Add(newReview);
            _context.SaveChanges();
            return Task.FromResult(newReview);
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

        // sort by review_date, rating, likes default = create_at
        public async Task<List<ReviewDTO>> GetReviewsByMovieId(int movieId, int page, int pageSize, string? orderBy)
        {
            
            using var connection = _dbConnectionFactory.CreateConnection();
            var reviews = await _context.Reviews
                .Where(r => r.MovieId == movieId)
                .Join(_context.Users,
                    review => review.UserId,
                    user => user.UserId,
                    (review, user) => new ReviewDTO
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
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            switch (orderBy)
            {
                case "review_date":
                    reviews = reviews.OrderByDescending(r => r.ReviewDate).ToList();
                    break;
                case "rating":
                    reviews = reviews.OrderByDescending(r => r.Rating).ToList();
                    break;
                case "likes":
                    reviews = reviews.OrderByDescending(r => r.Likes).ToList();
                    break;
                default:
                    reviews = reviews.OrderByDescending(r => r.ReviewDate).ToList();
                    break;
            }
            return reviews;
        }
        public Task<Review> UpdateReview(int reviewId, string newContent, int newRating)
        {
            var review = _context.Reviews.FirstOrDefault(r => r.ReviewId == reviewId);
            if (review != null)
            {
                review.ReviewContent = newContent;
                review.Rating = newRating;
                _context.SaveChanges();
                return Task.FromResult(review);
            }
            return Task.FromResult<Review>(null!); // Use null-forgiving operator to resolve CS8625  
        }
        public async Task<bool> UpdateReviewLike(int reviewId, string currentUserId)
        {
            try
            {
                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
                if (review == null)
                {
                    return false;
                }

                // Query with explicit handling of null/DBNull
                var existingUserReview = await _context.UserReviews
                    .Where(ur => ur.ReviewId == reviewId && ur.UserId == currentUserId)
                    .Select(ur => new
                    {
                        UserReview = ur,
                        ActionValue = ur.Action ?? string.Empty // Convert null to empty string
                    })
                    .FirstOrDefaultAsync();

                bool currentlyLiked = existingUserReview != null &&
                                     existingUserReview.ActionValue.Equals("like", StringComparison.OrdinalIgnoreCase);

                if (currentlyLiked)
                {
                    // Unlike
                    review.Likes = Math.Max(0, review.Likes - 1);
                    existingUserReview.UserReview.Action = null;
                    existingUserReview.UserReview.UpdateAt = DateTime.Now;
                }
                else
                {
                    // Like
                    review.Likes += 1;

                    if (existingUserReview != null)
                    {
                        existingUserReview.UserReview.Action = "like";
                        existingUserReview.UserReview.UpdateAt = DateTime.Now;
                    }
                    else
                    {
                        var newUserReview = new UserReview
                        {
                            UserId = currentUserId,
                            ReviewId = reviewId,
                            Action = "like",
                            UpdateAt = DateTime.Now
                        };
                        _context.UserReviews.Add(newUserReview);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating review like: {ex.Message}");
                return false;
            }
        }
        public async Task<bool> UpdateReviewUnlike(int reviewId, string currentUserId)
        {
            // Tương tự như UpdateReviewLike nhưng cho action = "unlike"
            try
            {
                var review = await _context.Reviews.FirstOrDefaultAsync(r => r.ReviewId == reviewId);
                if (review == null)
                {
                    return false;
                }

                // Query with explicit handling of null/DBNull
                var existingUserReview = await _context.UserReviews
                    .Where(ur => ur.ReviewId == reviewId && ur.UserId == currentUserId)
                    .Select(ur => new
                    {
                        UserReview = ur,
                        ActionValue = ur.Action ?? string.Empty // Convert null to empty string
                    })
                    .FirstOrDefaultAsync();

                bool currentlyLiked = existingUserReview != null &&
                                     existingUserReview.ActionValue.Equals("unlike", StringComparison.OrdinalIgnoreCase);

                if (currentlyLiked)
                {
                    // Unlike
                    review.Unlikes = Math.Max(0, review.Unlikes - 1);
                    existingUserReview.UserReview.Action = null;
                    existingUserReview.UserReview.UpdateAt = DateTime.Now;
                }
                else
                {
                    // Like
                    review.Unlikes += 1;

                    if (existingUserReview != null)
                    {
                        existingUserReview.UserReview.Action = "unlike";
                        existingUserReview.UserReview.UpdateAt = DateTime.Now;
                    }
                    else
                    {
                        var newUserReview = new UserReview
                        {
                            UserId = currentUserId,
                            ReviewId = reviewId,
                            Action = "unlike",
                            UpdateAt = DateTime.Now
                        };
                        _context.UserReviews.Add(newUserReview);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating review unlike: {ex.Message}");
                return false;
            }
        }
    }
}
