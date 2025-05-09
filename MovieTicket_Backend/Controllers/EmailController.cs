using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Models.ModelRequests;
using MovieTicket_Backend.Services;

namespace MovieTicket_Backend.Controllers
{
    [ApiController]
    [Route("api/v1/email")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send-verify")]
        public async Task<IActionResult> SendVerifyEmail([FromBody] EmailRequest request)
        {
            try
            {
                await _emailService.SendVerifyEmailAsync(request);
                return Ok(new { message = "Email sent successfully!" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Failed to send email", error = ex.Message });
            }
        }
    }
}
