namespace MovieTicket_Backend.Models
{
    public class SeatPrice
    {
        public string ShowingFormat { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }

}
