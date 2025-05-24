using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.Services
{
    public interface IWebSocketService
    {
        Task HandleWebSocketAsync(WebSocket webSocket, string connectionId);
        Task NotifySeatStatusChangeAsync(int showingId, SeatStatusUpdate update);
        Task NotifyBulkSeatStatusChangeAsync(int showingId, List<SeatStatusUpdate> updates);
    }

    public class WebSocketService : IWebSocketService
    {
        private readonly WebSocketConnectionManager _connectionManager;
        private readonly ILogger<WebSocketService> _logger;

        public WebSocketService(
            WebSocketConnectionManager connectionManager,
            ILogger<WebSocketService> logger)
        {
            _connectionManager = connectionManager;
            _logger = logger;
        }

        public async Task HandleWebSocketAsync(WebSocket webSocket, string connectionId)
        {
            var connection = new WebSocketConnection(connectionId, webSocket);
            _connectionManager.AddConnection(connection);

            try
            {
                await SendWelcomeMessage(connectionId);
                await ReceiveMessages(connection);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error handling WebSocket connection {connectionId}");
            }
            finally
            {
                await _connectionManager.RemoveConnectionAsync(connectionId);
            }
        }

        private async Task SendWelcomeMessage(string connectionId)
        {
            await _connectionManager.SendMessageAsync(connectionId, new
            {
                type = "connected",
                connectionId = connectionId,
                message = "WebSocket connection established",
                timestamp = DateTime.Now
            });
        }

        private async Task ReceiveMessages(WebSocketConnection connection)
        {
            var buffer = new byte[1024 * 4];

            while (connection.WebSocket.State == WebSocketState.Open)
            {
                try
                {
                    var result = await connection.WebSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        await ProcessMessage(connection, message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
                catch (WebSocketException ex)
                {
                    _logger.LogError(ex, $"WebSocket error for connection {connection.ConnectionId}");
                    break;
                }
            }
        }

        private async Task ProcessMessage(WebSocketConnection connection, string message)
        {
            try
            {
                var messageData = JsonSerializer.Deserialize<JsonElement>(message);

                if (!messageData.TryGetProperty("type", out var typeElement))
                {
                    await SendErrorMessage(connection.ConnectionId, "Message type is required");
                    return;
                }

                var messageType = typeElement.GetString();

                switch (messageType)
                {
                    case "joinShowing":
                        await HandleJoinShowing(connection, messageData);
                        break;
                    case "leaveShowing":
                        await HandleLeaveShowing(connection, messageData);
                        break;
                    case "ping":
                        await HandlePing(connection.ConnectionId);
                        break;
                    case "reserveSeat":
                        await HandleReserveSeat(connection, messageData);
                        break;
                    default:
                        await SendErrorMessage(connection.ConnectionId, $"Unknown message type: {messageType}");
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing message from {connection.ConnectionId}: {message}");
                await SendErrorMessage(connection.ConnectionId, "Invalid message format");
            }
        }

        private async Task HandleJoinShowing(WebSocketConnection connection, JsonElement messageData)
        {
            if (messageData.TryGetProperty("showingId", out var showingIdElement) &&
                showingIdElement.TryGetInt32(out var showingId))
            {
                if (messageData.TryGetProperty("userId", out var userIdElement))
                {
                    connection.UserId = userIdElement.GetString();
                }

                await _connectionManager.JoinShowingAsync(connection.ConnectionId, showingId);
            }
            else
            {
                await SendErrorMessage(connection.ConnectionId, "showingId is required");
            }
        }

        private async Task HandleLeaveShowing(WebSocketConnection connection, JsonElement messageData)
        {
            if (connection.ShowingId.HasValue)
            {
                await _connectionManager.LeaveShowingAsync(connection.ConnectionId, connection.ShowingId.Value);
            }
            else
            {
                await SendErrorMessage(connection.ConnectionId, "Not currently in any showing");
            }
        }

        private async Task HandlePing(string connectionId)
        {
            await _connectionManager.SendMessageAsync(connectionId, new
            {
                type = "pong",
                timestamp = DateTime.Now
            });
        }

        private async Task HandleReserveSeat(WebSocketConnection connection, JsonElement messageData)
        {
            if (!connection.ShowingId.HasValue)
            {
                await SendErrorMessage(connection.ConnectionId, "Must join showing first");
                return;
            }

            // This is just for real-time feedback - actual reservation happens via HTTP API
            if (messageData.TryGetProperty("seatId", out var seatIdElement) &&
                seatIdElement.TryGetInt32(out var seatId))
            {
                await _connectionManager.SendMessageAsync(connection.ConnectionId, new
                {
                    type = "seatReservationAttempt",
                    seatId = seatId,
                    showingId = connection.ShowingId.Value,
                    message = "Seat reservation attempt received. Please use HTTP API to complete reservation."
                });
            }
        }

        private async Task SendErrorMessage(string connectionId, string errorMessage)
        {
            await _connectionManager.SendMessageAsync(connectionId, new
            {
                type = "error",
                message = errorMessage,
                timestamp = DateTime.Now
            });
        }

        public async Task NotifySeatStatusChangeAsync(int showingId, SeatStatusUpdate update)
        {
            await _connectionManager.SendMessageToShowingAsync(showingId, new
            {
                type = "seatStatusUpdate",
                seatId = update.SeatId,
                status = update.Status.ToString(),
                reservedBy = update.ReservedBy,
                reservationExpiresAt = update.ReservationExpiresAt,
                timestamp = DateTime.Now
            });

            _logger.LogInformation($"Sent seat status update for seat {update.SeatId} in showing {showingId}");
        }

        public async Task NotifyBulkSeatStatusChangeAsync(int showingId, List<SeatStatusUpdate> updates)
        {
            await _connectionManager.SendMessageToShowingAsync(showingId, new
            {
                type = "bulkSeatStatusUpdate",
                updates = updates.Select(u => new
                {
                    seatId = u.SeatId,
                    status = u.Status.ToString(),
                    reservedBy = u.ReservedBy,
                    reservationExpiresAt = u.ReservationExpiresAt
                }),
                timestamp = DateTime.Now
            });

            _logger.LogInformation($"Sent bulk seat status update for {updates.Count} seats in showing {showingId}");
        }
    }
}