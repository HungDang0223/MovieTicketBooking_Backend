namespace MovieTicket_Backend.Models
{
    public class Discount
    {
        public int DiscountId { get; set; }
        public string DiscountMethod { get; set; } = string.Empty;
        public int DiscountPercentage { get; set; }
        public int DiscountAmount { get; set; }
        public int Quantity { get; set; }
        public DateTime ExpiredIn { get; set; }
    }
}
