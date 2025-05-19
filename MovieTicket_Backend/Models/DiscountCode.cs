namespace MovieTicket_Backend.Models
{
    public class DiscountCode
    {
        public int CodeId { get; set; }
        public int DiscountId { get; set; }
        public string DiscountCodeValue { get; set; } = null!;
        public bool IsUsed { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
