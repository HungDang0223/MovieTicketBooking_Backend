namespace MovieTicket_Backend.Models
{
    public class City
    {
        public int CityId { get; set; }
        public string CityName { get; set; } = string.Empty;

        public List<Cinema> Cinemas { get; set; } = new();
    }

}
