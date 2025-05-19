using Microsoft.EntityFrameworkCore;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Repositories;
using System.Data;

namespace MovieTicket_Backend.RepositoryImpl
{
    public class SeatReservationRepository : ISeatReservationRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly ILogger<SeatReservationRepository> _logger;
        private static readonly TimeSpan ReservationExpiration = TimeSpan.FromMinutes(10);

        public SeatReservationRepository(
            ApplicationDbContext dbContext,
            ILogger<SeatReservationRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<(bool Success, string Message)> ReserveSeatAsync(ReserveSeatRequest request)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync(IsolationLevel.RepeatableRead);
            try
            {
                // Tìm ghế với lock để tránh đặt đồng thời
                var seat = await _dbContext.ShowingSeats
                    .FromSqlRaw("SELECT * FROM showing_seat WHERE showing_id = {0} AND seat_id = {1} FOR UPDATE",
                        request.ShowingId, request.SeatId)
                    .FirstOrDefaultAsync();

                if (seat == null)
                {
                    return (false, "Ghế không tồn tại cho buổi chiếu này");
                }

                // Kiểm tra trạng thái ghế
                if (seat.Status != SeatStatus.Available)
                {
                    if (seat.Status == SeatStatus.Reserved &&
                        seat.ReservationExpiresAt < DateTime.Now)
                    {
                        // Đặt chỗ cũ đã hết hạn, có thể đặt lại
                        _logger.LogInformation($"Reservation expired for seat {seat.SeatId}, allowing new reservation");
                    }
                    else
                    {
                        return (false, "Ghế đã được đặt hoặc mua bởi người khác");
                    }
                }

                // Cập nhật trạng thái ghế
                seat.Status = SeatStatus.Reserved;
                seat.ReservedBy = request.UserId;
                seat.ReservedAt = DateTime.Now;
                seat.ReservationExpiresAt = DateTime.Now.Add(ReservationExpiration);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                // Tạo background job để tự động giải phóng ghế nếu không được xác nhận
                // Có thể sử dụng Hangfire hoặc Background Service
                // BackgroundJob.Schedule(() => ReleaseExpiredReservation(seat.Id), ReservationExpiration);

                return (true, $"Đã đặt ghế thành công, vui lòng thanh toán trong vòng {ReservationExpiration.TotalMinutes} phút");
            }
            catch (DbUpdateConcurrencyException ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Concurrency conflict when reserving seat");
                return (false, "Có lỗi xảy ra do nhiều người đang đặt cùng một ghế. Vui lòng thử lại");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error reserving seat");
                return (false, "Có lỗi xảy ra khi đặt ghế. \n" + ex);
            }
        }

        public async Task<bool> ConfirmReservationAsync(int showingId, int seatId, string userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var seat = await _dbContext.ShowingSeats
                    .Where(s => s.ShowingId == showingId && s.SeatId == seatId && s.ReservedBy == userId)
                    .FirstOrDefaultAsync();

                if (seat == null || seat.Status != SeatStatus.Reserved)
                {
                    return false;
                }

                // Chuyển trạng thái từ tạm đặt sang đã bán
                seat.Status = SeatStatus.Sold;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error confirming reservation");
                return false;
            }
        }

        public async Task<bool> CancelReservationAsync(int showingId, int seatId, string userId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var seat = await _dbContext.ShowingSeats
                    .Where(s => s.ShowingId == showingId && s.SeatId == seatId && s.ReservedBy == userId)
                    .FirstOrDefaultAsync();

                if (seat == null || (seat.Status != SeatStatus.Reserved && seat.Status != SeatStatus.Reserved))
                {
                    return false;
                }

                // Giải phóng ghế
                seat.Status = SeatStatus.Available;
                seat.ReservedBy = null;
                seat.ReservedAt = null;
                seat.ReservationExpiresAt = null;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error canceling reservation");
                return false;
            }
        }

        // Phương thức giải phóng ghế hết hạn - được gọi bởi background service
        public async Task ReleaseExpiredReservation(int seatId)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var seat = await _dbContext.ShowingSeats.FindAsync(seatId);

                if (seat != null && seat.Status == SeatStatus.Reserved &&
                    seat.ReservationExpiresAt < DateTime.Now)
                {
                    seat.Status = SeatStatus.Available;
                    seat.ReservedBy = null;
                    seat.ReservedAt = null;
                    seat.ReservationExpiresAt = null;

                    await _dbContext.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _logger.LogInformation($"Released expired reservation for seat {seatId}");
                }
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error releasing expired reservation");
            }
        }
    }
}
