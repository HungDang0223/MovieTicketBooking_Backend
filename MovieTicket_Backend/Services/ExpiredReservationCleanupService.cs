using Microsoft.EntityFrameworkCore;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.Services
{
    public class ExpiredReservationCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<ExpiredReservationCleanupService> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1);

        public ExpiredReservationCleanupService(
            IServiceProvider serviceProvider,
            ILogger<ExpiredReservationCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Expired Reservation Cleanup Service is starting");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredReservations(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error cleaning up expired reservations");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CleanupExpiredReservations(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var now = DateTime.UtcNow;

            // Tìm tất cả các ghế đã hết hạn
            var expiredSeats = await dbContext.ShowingSeats
                .Where(s => s.Status == SeatStatus.TemporarilyReserved && s.ReservationExpiresAt < now)
                .ToListAsync(cancellationToken);

            if (expiredSeats.Any())
            {
                _logger.LogInformation($"Found {expiredSeats.Count} expired reservations to clean up");

                using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    foreach (var seat in expiredSeats)
                    {
                        seat.Status = SeatStatus.Available;
                        seat.ReservedBy = null;
                        seat.ReservedAt = null;
                        seat.ReservationExpiresAt = null;
                    }

                    await dbContext.SaveChangesAsync(cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    _logger.LogInformation($"Successfully cleaned up {expiredSeats.Count} expired reservations");
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    _logger.LogError(ex, "Error cleaning up expired reservations");
                }
            }
        }
    }
}
