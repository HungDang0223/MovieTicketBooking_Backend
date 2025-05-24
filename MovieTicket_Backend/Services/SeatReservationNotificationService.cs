using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.Services
{
    public interface ISeatReservationNotificationService
    {
        Task NotifySeatStatusChangeAsync(int showingId, SeatStatusUpdate update);
        Task NotifyBulkSeatStatusChangeAsync(int showingId, List<SeatStatusUpdate> updates);
    }

    public class SeatReservationNotificationService : ISeatReservationNotificationService
    {
        private readonly IWebSocketService _webSocketService;
        private readonly ILogger<SeatReservationNotificationService> _logger;

        public SeatReservationNotificationService(
            IWebSocketService webSocketService,
            ILogger<SeatReservationNotificationService> logger)
        {
            _webSocketService = webSocketService;
            _logger = logger;
        }

        public async Task NotifySeatStatusChangeAsync(int showingId, SeatStatusUpdate update)
        {
            await _webSocketService.NotifySeatStatusChangeAsync(showingId, update);
            _logger.LogInformation($"Notified seat status change for seat {update.SeatId} in showing {showingId}");
        }

        public async Task NotifyBulkSeatStatusChangeAsync(int showingId, List<SeatStatusUpdate> updates)
        {
            await _webSocketService.NotifyBulkSeatStatusChangeAsync(showingId, updates);
            _logger.LogInformation($"Notified bulk seat status change for {updates.Count} seats in showing {showingId}");
        }
    }
}