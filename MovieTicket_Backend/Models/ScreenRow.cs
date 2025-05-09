namespace MovieTicket_Backend.Models
{
    public class ScreenRow
    {
        public int RowId { get; set; }
        public string RowName { get; set; } = string.Empty;
        public string SeatType { get; set; } = string.Empty;
        public int SeatTotal { get; set; }
        public int ScreenId { get; set; }
        public List<Seat> Seats { get; set; } = new();
    }

}
