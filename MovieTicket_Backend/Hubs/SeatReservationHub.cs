using Microsoft.AspNetCore.SignalR;
using MovieTicket_Backend.Models;
using System.Threading.Tasks;

namespace MovieTicket_Backend.Hubs
{
    public class SeatReservationHub : Hub
    {
        private readonly ILogger<SeatReservationHub> _logger;

        public SeatReservationHub(ILogger<SeatReservationHub> logger)
        {
            _logger = logger;
        }

        // Client gọi phương thức này để tham gia group theo screeningId
        public async Task JoinScreening(int screeningId)
        {
            string groupName = GetScreeningGroupName(screeningId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} joined screening group {screeningId}");
        }

        // Client gọi phương thức này để rời khỏi group
        public async Task LeaveScreening(int screeningId)
        {
            string groupName = GetScreeningGroupName(screeningId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} left screening group {screeningId}");
        }

        // Helper method để tạo tên group
        private string GetScreeningGroupName(int screeningId)
        {
            return $"screening_{screeningId}";
        }

        // Xử lý khi client ngắt kết nối
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client {Context.ConnectionId} disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}