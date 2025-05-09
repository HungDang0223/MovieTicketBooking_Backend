namespace MovieTicket_Backend.Models
{
    public class ShowingSeat
    {
        public int Id { get; set; }
        public int ShowingId { get; set; }
        public int SeatId { get; set; }
        public SeatStatus Status { get; set; }
        public string? ReservedBy { get; set; }
        public DateTime? ReservedAt { get; set; }
        public DateTime? ReservationExpiresAt { get; set; }
    }
    public enum SeatStatus
    {
        Available,
        TemporarilyReserved,
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
