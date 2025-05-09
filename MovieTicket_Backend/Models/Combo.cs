namespace MovieTicket_Backend.Models
{
    public class Combo
    {
        public int ComboId { get; set; }
        public string ComboName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Price { get; set; }
        public string ImageUrl { get; set; } = string.Empty;
    }
}
