namespace MovieTicket_Backend.Models.ModelDTOs
{
    public class MovieDetailDTO
    {
        public string Title { get; set; } = string.Empty;
        public DateTime? ReleaseDate { get; set; }
        public int Duration { get; set; }
        public double? Rating { get; set; }
        public string? Synopsis { get; set; }
        public string? PosterUrl { get; set; }
        public string? TrailerUrl { get; set; }
        public string? Censor { get; set; }
        public string? Cast { get; set; }
        public string? Directors { get; set; }
        public string? Genre { get; set; }
        public DateTime? ShowingDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int FavouritesCount { get; set; } = 0;
        public List<ReviewDTO> Reviews { get; set; } = new();
        public bool IsFavourited { get; set; } = false;
    }
}
