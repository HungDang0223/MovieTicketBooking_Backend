using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Repositories;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TicketController : Controller
    {
        private readonly ITicketRepository _ticketRepository;
        public TicketController(ITicketRepository ticketRepository)
        {
            _ticketRepository = ticketRepository;
        }
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetTicketsByUserId(string userId)
        {
            var tickets = await _ticketRepository.GetTicketsByUserId(userId);
            if (tickets == null || tickets.Any())
            {
                return Ok(new { status = "success", message = "No tickets found" });
            }
            else
            {
                return Ok(new { status = "success", data = tickets });
            }
        }
    }
}
