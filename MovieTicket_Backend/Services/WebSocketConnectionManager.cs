using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace MovieTicket_Backend.Services
{
    public class WebSocketConnection
    {
        public string ConnectionId { get; set; }
        public WebSocket WebSocket { get; set; }
        public int? ShowingId { get; set; }
        public string UserId { get; set; }
        public DateTime ConnectedAt { get; set; }

        public WebSocketConnection(string connectionId, WebSocket webSocket)
        {
            ConnectionId = connectionId;
            WebSocket = webSocket;
            ConnectedAt = DateTime.Now;
        }
    }

    public class WebSocketConnectionManager
    {
        private readonly ConcurrentDictionary<string, WebSocketConnection> _connections = new();
        private readonly ConcurrentDictionary<int, List<string>> _showingRooms = new();
        private readonly ILogger<WebSocketConnectionManager> _logger;

        public WebSocketConnectionManager(ILogger<WebSocketConnectionManager> logger)
        {
            _logger = logger;
        }

        public void AddConnection(WebSocketConnection connection)
        {
            _connections.TryAdd(connection.ConnectionId, connection);
            _logger.LogInformation($"WebSocket connection added: {connection.ConnectionId}");
        }

        public async Task RemoveConnectionAsync(string connectionId)
        {
            if (_connections.TryRemove(connectionId, out var connection))
            {
                if (connection.ShowingId.HasValue)
                {
                    await LeaveShowingAsync(connectionId, connection.ShowingId.Value);
                }

                if (connection.WebSocket.State == WebSocketState.Open)
                {
                    await connection.WebSocket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Connection closed",
                        CancellationToken.None);
                }

                connection.WebSocket?.Dispose();
                _logger.LogInformation($"WebSocket connection removed: {connectionId}");
            }
        }

        public async Task JoinShowingAsync(string connectionId, int showingId)
        {
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                // Leave previous showing if any
                if (connection.ShowingId.HasValue)
                {
                    await LeaveShowingAsync(connectionId, connection.ShowingId.Value);
                }

                connection.ShowingId = showingId;

                _showingRooms.AddOrUpdate(showingId,
                    new List<string> { connectionId },
                    (key, existingConnections) =>
                    {
                        if (!existingConnections.Contains(connectionId))
                            existingConnections.Add(connectionId);
                        return existingConnections;
                    });

                await SendMessageAsync(connectionId, new
                {
                    type = "joinedShowing",
                    showingId = showingId,
                    message = "Successfully joined showing"
                });

                _logger.LogInformation($"Connection {connectionId} joined showing {showingId}");
            }
        }

        public async Task LeaveShowingAsync(string connectionId, int showingId)
        {
            if (_connections.TryGetValue(connectionId, out var connection))
            {
                connection.ShowingId = null;

                if (_showingRooms.TryGetValue(showingId, out var connections))
                {
                    connections.Remove(connectionId);
                    if (!connections.Any())
                    {
                        _showingRooms.TryRemove(showingId, out _);
                    }
                }

                await SendMessageAsync(connectionId, new
                {
                    type = "leftShowing",
                    showingId = showingId,
                    message = "Successfully left showing"
                });

                _logger.LogInformation($"Connection {connectionId} left showing {showingId}");
            }
        }

        public async Task SendMessageAsync(string connectionId, object message)
        {
            if (_connections.TryGetValue(connectionId, out var connection) &&
                connection.WebSocket.State == WebSocketState.Open)
            {
                try
                {
                    var json = JsonSerializer.Serialize(message);
                    var bytes = Encoding.UTF8.GetBytes(json);
                    var buffer = new ArraySegment<byte>(bytes);

                    await connection.WebSocket.SendAsync(
                        buffer,
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending message to connection {connectionId}");
                    await RemoveConnectionAsync(connectionId);
                }
            }
        }

        public async Task SendMessageToShowingAsync(int showingId, object message)
        {
            if (_showingRooms.TryGetValue(showingId, out var connectionIds))
            {
                var tasks = connectionIds.Select(connectionId => SendMessageAsync(connectionId, message));
                await Task.WhenAll(tasks);
            }
        }

        public WebSocketConnection GetConnection(string connectionId)
        {
            _connections.TryGetValue(connectionId, out var connection);
            return connection;
        }

        public int GetConnectionCount()
        {
            return _connections.Count;
        }

        public int GetShowingConnectionCount(int showingId)
        {
            return _showingRooms.TryGetValue(showingId, out var connections) ? connections.Count : 0;
        }
    }
}