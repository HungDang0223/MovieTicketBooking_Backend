namespace MovieTicket_Backend.Models.ModelDTOs
{
    public class ShowingMovieDTO
    {
        public int ShowingId { get; set; }
        public int ScreenId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime ShowingDate { get; set; }
        public string ShowingFormat { get; set; }
        public string CinemaName { get; set; }
        public string ScreenName { get; set; }
        public string Language { get; set; } = string.Empty;
        public string SubtitleLanguage { get; set; } = string.Empty;
        public int SeatCount { get; set; }
    }
}
