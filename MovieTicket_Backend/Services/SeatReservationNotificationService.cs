using Microsoft.AspNetCore.SignalR;
using MovieTicket_Backend.Hubs;
using MovieTicket_Backend.Models;
using System.Threading.Tasks;

namespace MovieTicket_Backend.Services
{
    public interface ISeatReservationNotificationService
    {
        Task NotifySeatStatusChangeAsync(int screeningId, SeatStatusUpdate update);
        Task NotifyBulkSeatStatusChangeAsync(int screeningId, List<SeatStatusUpdate> updates);
    }

    public class SeatReservationNotificationService : ISeatReservationNotificationService
    {
        private readonly IHubContext<SeatReservationHub> _hubContext;
        private readonly ILogger<SeatReservationNotificationService> _logger;

        public SeatReservationNotificationService(
            IHubContext<SeatReservationHub> hubContext,
            ILogger<SeatReservationNotificationService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        // Gửi thông báo về việc thay đổi trạng thái một ghế
        public async Task NotifySeatStatusChangeAsync(int screeningId, SeatStatusUpdate update)
        {
            string groupName = GetScreeningGroupName(screeningId);

            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveSeatUpdate", update);

            _logger.LogInformation($"Sent seat status update for seat {update.SeatId} in screening {screeningId}");
        }

        // Gửi thông báo về việc thay đổi trạng thái nhiều ghế cùng lúc
        public async Task NotifyBulkSeatStatusChangeAsync(int screeningId, List<SeatStatusUpdate> updates)
        {
            string groupName = GetScreeningGroupName(screeningId);

            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveBulkSeatUpdate", updates);

            _logger.LogInformation($"Sent bulk seat status update for {updates.Count} seats in screening {screeningId}");
        }

        private string GetScreeningGroupName(int screeningId)
        {
            return $"screening_{screeningId}";
        }
    }
}