namespace MovieTicket_Backend.Models
{
    public class UserReview
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public int ReviewId { get; set; }
        public string? Action { get; set; } // "like" or "unlike" default = null, null nếu user không like & unlike
        public DateTime UpdateAt { get; set; }
    }
}
