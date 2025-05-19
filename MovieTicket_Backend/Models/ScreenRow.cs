using System.ComponentModel.DataAnnotations.Schema;

namespace MovieTicket_Backend.Models
{
    public class ScreenRow
    {
        [Column("row_id")]
        public int RowId { get; set; }
        [Column("row_name")]
        public string RowName { get; set; } = string.Empty;
        [Column("seat_type")]
        public string SeatType { get; set; } = string.Empty;
        [Column("seat_count")]
        public int SeatCount { get; set; }
        [Column("screen_id")]
        public int ScreenId { get; set; }
        public List<Seat> Seats { get; set; } = new();
    }

}
