using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.RepositoryImpl;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/[controller]")]
    public class ScreenController : Controller
    {
        private readonly ILogger<ScreenController> _logger;
        private readonly ScreenRepository _screenRepository;
        public ScreenController(ILogger<ScreenController> logger, ScreenRepository screenRepository)
        {
            _logger = logger;
            _screenRepository = screenRepository;
        }
        [HttpGet("/{screenId}/seats")]
        public async Task<IActionResult> GetScreenSeats(int screenId)
        {
            try
            {
                var result = await _screenRepository.GetScreenSeats(screenId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting screen seats");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
