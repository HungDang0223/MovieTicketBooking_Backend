using System.ComponentModel.DataAnnotations.Schema;

namespace MovieTicket_Backend.Models
{
    [Table("review")]
    public class Review
    {
        [Column("review_id")]
        public int ReviewId { get; set; }
        [Column("user_id")]
        public string UserId { get; set; }
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

}
