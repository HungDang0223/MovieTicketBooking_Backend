namespace MovieTicket_Backend.Models
{
    public class Screen
    {
        public int ScreenId { get; set; }
        public string ScreenName { get; set; } = string.Empty;
        public int CinemaId { get; set; }
        public string AislePos { get; set; } = string.Empty; // Stored as JSON

        public Cinema Cinema { get; set; }
        public List<ScreenRow> ScreenRows { get; set; } = new();
        public List<ShowingMovie> MovieShowings { get; set; } = new();
    }

}
