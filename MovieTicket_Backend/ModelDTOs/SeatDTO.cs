namespace MovieTicket_Backend.ModelDTOs
{
    public class RowSeatsDto
    {
        public string RowName { get; set; }
        public string SeatType { get; set; }
        public List<SeatDto> Seats { get; set; }
    }

    public class SeatDto
    {
        public int SeatId { get; set; }
        public int SeatNumber { get; set; }
    }
}
