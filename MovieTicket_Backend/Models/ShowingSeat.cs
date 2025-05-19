using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MovieTicket_Backend.Models
{
    public class ShowingSeat
    {
        [Column("id")]
        public int Id { get; set; }

        [Column("showing_id")]
        public int ShowingId { get; set; }

        [Column("seat_id")]
        public int SeatId { get; set; }
        [Column("status")]
        public SeatStatus Status { get; set; }
        [Column("reserved_by")]
        public string? ReservedBy { get; set; }
        [Column("reserved_at")]
        public DateTime? ReservedAt { get; set; }
        [Column("reservation_expires_at")]
        public DateTime? ReservationExpiresAt { get; set; }
    }
    public enum SeatStatus
    {
        Available,
        Reserved,
        Sold
    }

    // 2. Model cho request đặt ghế
    public class ReserveSeatRequest
    {
        public int ShowingId { get; set; }
        public int SeatId { get; set; }
        public string UserId { get; set; }
    }

    // DTO để gửi thông tin cập nhật trạng thái ghế
    public class SeatStatusUpdate
    {
        public int SeatId { get; set; }
        public SeatStatus Status { get; set; }
        public int? ReservedBy { get; set; }
        public DateTime? ReservationExpiresAt { get; set; }
    }
}
