namespace MovieTicket_Backend.Models
{
    public class Ticket
    {
        public int BookingId { get; set; }
        public string UserId { get; set; }
        public int ShowingId { get; set; }
        public DateTime BookingDate { get; set; } = DateTime.Now;
        public int BookingAmount { get; set; }
        public string BookingStatus { get; set; } = "Pending";
        public int DiscountId { get; set; }

        public ShowingMovie ShowingMovie { get; set; } = new();
        public List<BookingSeat> BookingSeats { get; set; } = new();
        public List<BookingSnack> BookingSnacks { get; set; } = new();
        public List<BookingCombo> BookingCombos { get; set; } = new();
    }

}
