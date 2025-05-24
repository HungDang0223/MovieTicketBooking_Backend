using MovieTicket_Backend.Services;
using System.Net.WebSockets;

namespace MovieTicket_Backend.Middleware
{
    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IWebSocketService _webSocketService;
        private readonly ILogger<WebSocketMiddleware> _logger;

        public WebSocketMiddleware(
            RequestDelegate next,
            IWebSocketService webSocketService,
            ILogger<WebSocketMiddleware> logger)
        {
            _next = next;
            _webSocketService = webSocketService;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path == "/ws/seat-reservation")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    var webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    var connectionId = Guid.NewGuid().ToString();

                    _logger.LogInformation($"WebSocket connection established: {connectionId}");
                    await _webSocketService.HandleWebSocketAsync(webSocket, connectionId);
                }
                else
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("WebSocket connection required");
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}