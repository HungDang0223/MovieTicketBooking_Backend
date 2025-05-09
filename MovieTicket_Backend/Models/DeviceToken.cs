using System.ComponentModel.DataAnnotations.Schema;

namespace MovieTicket_Backend.Models
{
    public class DeviceToken
    {
        [Column("id")]
        public int Id { get; set; }
        [Column("token")]
        public string Token { get; set; }
        [Column("user_id")]
        public string UserId { get; set; }
        [Column("device_type")]
        public string DeviceType { get; set; } // "android", "ios", "web"
        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
        [Column("last_updated")]
        public DateTime? LastUpdated { get; set; }

        public User User { get; set; }
    }
}
