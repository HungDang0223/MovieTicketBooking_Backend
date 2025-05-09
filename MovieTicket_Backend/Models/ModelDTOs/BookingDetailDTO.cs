namespace MovieTicket_Backend.Models.ModelDTOs
{
    public class BookingDetailDTO
    {
        public string Title { get; set; } = string.Empty;
        public string ScreenName { get; set; } = string.Empty;
        public List<string> SeatNames { get; set; } = new();
        public List<SnackDTO> Snacks { get; set; } = new();
        public List<ComboDTO> Combos { get; set; } = new();
    }

    public class SnackDTO
    {
        public string SnackName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }

    public class ComboDTO
    {
        public string ComboName { get; set; } = string.Empty;
        public int Quantity { get; set; }
    }
}
