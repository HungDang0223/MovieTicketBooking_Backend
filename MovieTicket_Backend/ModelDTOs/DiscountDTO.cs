namespace MovieTicket_Backend.ModelDTOs
{
    public class DiscountDTO
    {
        public string? DiscountCode { get; set; }  // có thể null
        public string? DiscountDescription { get; set; }
        public string DiscountType { get; set; } = null!;  // 'percent', 'fixed', 'point'
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal? MinOrderValue { get; set; }
        public int Quantity { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
    public class ApplyDiscountRequest
    {
        public string DiscountCode { get; set; }
        public string UserId { get; set; }
    }
}
