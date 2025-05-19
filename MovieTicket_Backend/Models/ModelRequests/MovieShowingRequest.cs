namespace MovieTicket_Backend.Models.ModelRequests
{
    public class MovieShowingRequest
    {
        public int MovieId { get; set; }
        public DateOnly ShowingDate { get; set; }
    }
}
