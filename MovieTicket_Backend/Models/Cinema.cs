namespace MovieTicket_Backend.Models
{
    public class Cinema
    {
        public int CinemaId { get; set; }
        public string CinemaName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public int CityId { get; set; }
        public string ImagePath { get; set; }
        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }
        public string CityName { get; set; }
    }

}
