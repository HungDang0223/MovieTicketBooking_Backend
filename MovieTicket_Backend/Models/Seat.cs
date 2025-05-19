using System.ComponentModel.DataAnnotations.Schema;

namespace MovieTicket_Backend.Models
{
    public class Seat
    {
        [Column("seat_id")]
        public int SeatId { get; set; }
        [Column("row_id")]
        public int RowId { get; set; }
        [Column("seat_number")]
        public int SeatNumber { get; set; }

        public ScreenRow ScreenRow { get; set; }
    }

}
