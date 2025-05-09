namespace MovieTicket_Backend.Models.ModelDTOs
{
    public class MovieShowingDTO
    {
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime ShowingDate { get; set; }
        public string ShowingFormat { get; set; }
        public string CinemaName { get; set; }
        public string ScreenName { get; set; }
    }
}
