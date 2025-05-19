namespace MovieTicket_Backend.Models.ModelDTOs
{
    public class CreateReviewDTO
    {
        public string UserId{ get; set; }
        public int MovieId { get; set; }
        public int Rating { get; set; }
        public string ReviewContent { get; set; } = String.Empty;
        public DateTime ReviewDate { get; set; } = DateTime.Now;
    }
    public class UpdateReviewDTO
    {
        public string NewContent { get; set; }
        public int NewRating { get; set; }
    }
}
