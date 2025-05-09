namespace MovieTicket_Backend.Models
{
    public class BookingSeat
    {
        public int BookingId { get; set; }
        public int SeatId { get; set; }

        public Seat Seat { get; set; } = new();
    }
}
