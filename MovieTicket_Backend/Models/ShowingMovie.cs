using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieTicket_Backend.Models
{
    public class ShowingMovie
    {
        [Column("showing_id")]
        public int ShowingId { get; set; }
        public int MovieId { get; set; }
        public int ScreenId { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public DateTime ShowingDate { get; set; }
        public string ShowingFormat { get; set; } = string.Empty;

        public Movie Movie { get; set; }
        public Screen Screen { get; set; }
    }

}
