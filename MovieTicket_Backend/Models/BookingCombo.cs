namespace MovieTicket_Backend.Models
{
    public class BookingCombo
    {
        public int BookingId { get; set; }
        public int ComboId { get; set; }
        public int Quantity { get; set; }
        public Combo Combo { get; set; } = new();
    }

}
