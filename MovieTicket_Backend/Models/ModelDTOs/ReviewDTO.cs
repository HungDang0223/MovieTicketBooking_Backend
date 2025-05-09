namespace MovieTicket_Backend.Models.ModelDTOs
{
    public class ReviewDTO
    {
        public string UserName { get; set; }
        public string UserRank { get; set; }
        public int Rating { get; set; }
        public string ReviewContent { get; set; } = String.Empty;
        public DateTime ReviewDate { get; set; } = DateTime.Now;
    }
}
