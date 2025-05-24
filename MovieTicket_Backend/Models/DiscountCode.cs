namespace MovieTicket_Backend.Models
{
    public class DiscountCode
    {
        public int Id { get; set; }
        public int DiscountId { get; set; }
        public string Code { get; set; }
        public bool IsUsed { get; set; }
        public string? UsedBy { get; set; } // user_id
        public DateTime UsedAt { get; set; }
    }
}
