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

        // Client gọi phương thức này để tham gia group theo showingId
        public async Task JoiShowing(int showingId)
        {
            string groupName = GetShowingGroupName(showingId);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} joined Showing group {showingId}");
        }

        // Client gọi phương thức này để rời khỏi group
        public async Task LeaveShowing(int showingId)
        {
            string groupName = GetShowingGroupName(showingId);
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
            _logger.LogInformation($"Client {Context.ConnectionId} left Showing group {showingId}");
        }

        // Helper method để tạo tên group
        private string GetShowingGroupName(int showingId)
        {
            return $"Showing_{showingId}";
        }

        // Xử lý khi client ngắt kết nối
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client {Context.ConnectionId} disconnected");
            await base.OnDisconnectedAsync(exception);
        }
    }
}