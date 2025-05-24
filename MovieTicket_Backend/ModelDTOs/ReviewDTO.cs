using System.ComponentModel.DataAnnotations.Schema;

namespace MovieTicket_Backend.ModelDTOs
{
    public class ReviewDTO
    {
        [Column("review_id")]
        public int ReviewId { get; set; }
        [Column("user_id")]
        public string UserId { get; set; }
        [Column("full_name")]
        public string FullName { get; set; }
        [Column("photo_path")]
        public string PhotoPath { get; set; }
        [Column("movie_id")]
        public int MovieId { get; set; }
        [Column("rating")]
        public int Rating { get; set; }
        [Column("review_content")]
        public string? ReviewContent { get; set; }
        [Column("review_date")]
        public DateTime ReviewDate { get; set; } = DateTime.Now;
        [Column("likes")]
        public int Likes { get; set; }
        [Column("unlikes")]
        public int Unlikes { get; set; }
    }
    public class CreateReviewDTO
    {
        public string UserId { get; set; }
        public int MovieId { get; set; }
        public int Rating { get; set; }
        public string ReviewContent { get; set; } = string.Empty;
        public DateTime ReviewDate { get; set; } = DateTime.Now;
    }
    public class UpdateReviewDTO
    {
        public string NewContent { get; set; }
        public int NewRating { get; set; }
    }
}
