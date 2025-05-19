namespace MovieTicket_Backend.Models
{
    public class Discount
    {
        public int DiscountId { get; set; }
        public string? DiscountCode { get; set; }  // có thể null
        public string? DiscountDescription { get; set; }
        public string DiscountType { get; set; } = null!;  // 'percent', 'fixed', 'point'
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? MinOrderValue { get; set; }
        public int Quantity { get; set; }
        public bool IsActive { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
