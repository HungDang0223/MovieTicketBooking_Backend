using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieTicket_Backend.Models
{
    public class ShowingMovie
    {
        [Column("showing_id")]
        public int ShowingId { get; set; }
        [Column("movie_id")]
        public int MovieId { get; set; }
        [Column("screen_id")]
        public int ScreenId { get; set; }
        [Column("start_time")]
        public TimeSpan StartTime { get; set; }
        [Column("end_time")]
        public TimeSpan EndTime { get; set; }
        [Column("showing_date")]
        public DateTime ShowingDate { get; set; }
        [Column("showing_format")]
        public string ShowingFormat { get; set; } = string.Empty;
        [Column("language")]
        public string Language { get; set; } = string.Empty;
        [Column("subtitle_language")]
        public string SubtitleLanguage { get; set; } = string.Empty;

        public Movie Movie { get; set; }
        public Screen Screen { get; set; }
    }

}
