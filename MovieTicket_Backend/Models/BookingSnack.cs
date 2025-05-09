namespace MovieTicket_Backend.Models
{
    public class BookingSnack
    {
        public int BookingId { get; set; }
        public int SnackId { get; set; }
        public int Quantity { get; set; }
        public Snack Snack { get; set; } = new();
    }

}
