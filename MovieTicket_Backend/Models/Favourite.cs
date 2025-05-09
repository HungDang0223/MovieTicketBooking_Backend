namespace MovieTicket_Backend.Models
{
    public class Favourite
    {
        public int UserId { get; set; }
        public int MovieId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.Now;
        public User User { get; set; }
        public Movie Movie { get; set; }
    }

}
