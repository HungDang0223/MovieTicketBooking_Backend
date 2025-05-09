using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Services;

namespace MovieTicket_Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IDeviceTokenService _deviceTokenService;

        public NotificationController(
            INotificationService notificationService,
            IDeviceTokenService deviceTokenService)
        {
            _notificationService = notificationService;
            _deviceTokenService = deviceTokenService;
        }

        [HttpPost("register-token")]
        public async Task<IActionResult> RegisterToken(string userId, string token, string deviceType)
        {
            var result = await _deviceTokenService.RegisterDeviceToken(userId, token, deviceType);
            return Ok(new { success = result });
        }

        [HttpGet("test-notification/{userId}")]
        public async Task<IActionResult> TestNotification(string userId)
        {
            var tokens = await _deviceTokenService.GetUserTokens(userId);

            if (tokens.Count == 0)
                return NotFound("No device tokens found for this user");

            await _notificationService.SendNewMovieNotification(
                "Test Movie",
                "This is a test notification",
                tokens);

            return Ok(new { message = "Test notification sent" });
        }
    }
}
