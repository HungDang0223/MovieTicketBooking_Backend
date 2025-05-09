namespace MovieTicket_Backend.Models
{
    public class Cinema
    {
        public int CinemaId { get; set; }
        public string CinemaName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Website { get; set; } = string.Empty;
        public int BrandId { get; set; }
        public int CityId { get; set; }
        public Brand Brand { get; set; } = new();
        public City City { get; set; } = new();
        public List<Screen> Screens { get; set; } = new();
    }

}
