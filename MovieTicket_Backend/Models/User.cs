using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace MovieTicket_Backend.Models
{
    [Table("user")]
    public class User
    {
        [Key]
        [Column("user_id")]
        public string UserId { get; set; }

        [Column("full_name")]
        public string FullName { get; set; } = string.Empty;

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Column("password_hash")]
        public string Password { get; set; } = string.Empty;

        [Column("date_of_birth")]
        public DateTime DateOfBirth { get; set; }

        [Column("gender")]
        public string Gender { get; set; } = "Nam";

        [Column("address")]
        public string? Address { get; set; }
        [Column("photo_url")]
        public string? PhotoUrl { get; set; }

        [Column("account_status")]
        public string AccountStatus { get; set; } = "active";
        [Column("role")]
        public string Role { get; set; } = "user";
        [Column("is_deleted")]
        public bool IsDeleted { get; set; } = false;
        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
        [Column("rank_id")]
        public int RankId { get; set; } = 1;
        [Column("total_point")]
        public int TotalPoint { get; set; } = 0;
        [Column("total_paid")]
        public int TotalPaid { get; set; } = 0;

        [Column("refresh_token")]
        public string RefreshToken { get; set; }

        [Column("refresh_token_expiry")]
        public DateTime? RefreshTokenExpiry { get; set; }
    }
}
