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

        [HttpGet("{screenId}/seats")]
        public async Task<IActionResult> GetSeatsByScreen(int screenId)
        {
            var rowSeats = await _screenRepository.GetSeatsByScreenIdAsync(screenId);
            if (rowSeats == null || !rowSeats.Any())
            {
                return NotFound($"Không tìm thấy ghế cho screen_id = {screenId}");
            }
            return Ok(rowSeats);
        }
    }
}
