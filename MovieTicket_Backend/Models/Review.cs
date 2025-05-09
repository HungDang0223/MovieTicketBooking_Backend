namespace MovieTicket_Backend.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public string UserId { get; set; }
        public int MovieId { get; set; }
        public decimal Rating { get; set; }
        public string? ReviewContent { get; set; }
        public DateTime ReviewDate { get; set; } = DateTime.Now;
    }

}
