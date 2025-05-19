using Microsoft.AspNetCore.SignalR;
using MovieTicket_Backend.Hubs;
using MovieTicket_Backend.Models;
using System.Threading.Tasks;

namespace MovieTicket_Backend.Services
{
    public interface ISeatReservationNotificationService
    {
        Task NotifySeatStatusChangeAsync(int showingId, SeatStatusUpdate update);
        Task NotifyBulkSeatStatusChangeAsync(int showingId, List<SeatStatusUpdate> updates);
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
        public async Task NotifySeatStatusChangeAsync(int showingId, SeatStatusUpdate update)
        {
            string groupName = GetshowingGroupName(showingId);

            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveSeatUpdate", update);

            _logger.LogInformation($"Sent seat status update for seat {update.SeatId} in showing {showingId}");
        }

        // Gửi thông báo về việc thay đổi trạng thái nhiều ghế cùng lúc
        public async Task NotifyBulkSeatStatusChangeAsync(int showingId, List<SeatStatusUpdate> updates)
        {
            string groupName = GetshowingGroupName(showingId);

            await _hubContext.Clients.Group(groupName)
                .SendAsync("ReceiveBulkSeatUpdate", updates);

            _logger.LogInformation($"Sent bulk seat status update for {updates.Count} seats in showing {showingId}");
        }

        private string GetshowingGroupName(int showingId)
        {
            return $"showing_{showingId}";
        }
    }
}