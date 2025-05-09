using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.RepositoryInpl;

namespace MovieTicket_Backend.Controllers
{
    [Route("api/v1/cinema")]
    [ApiController]
    public class CinemaController : Controller
    {
        private readonly CinemaRepository _cinemaRepository;
        private readonly PasswordHasher<string> _passwordHasher;
        private readonly DbConnectionFactory _dbConnectionFactory;

        public CinemaController(CinemaRepository cinemaRepository, DbConnectionFactory dbConnectionFactory)
        {
            _cinemaRepository = cinemaRepository;
            _passwordHasher = new PasswordHasher<string>();
            _dbConnectionFactory = dbConnectionFactory;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllCinemas()
        {
            var connection = _dbConnectionFactory.CreateConnection();
            var cinemas = await _cinemaRepository.GetAllCinemas();
            if (cinemas == null || cinemas.Count == 0)
            {
                return NotFound(new { status = "error", message = "No cinemas found" });
            }
            return Ok(new { status = "success", data = cinemas });
        }
        [HttpGet("brand/{id}")]
        public async Task<IActionResult> GetCinemasByBrandId(int id)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            var cinema = await _cinemaRepository.GetCinemasByBrandId(id);
            if (cinema == null)
            {
                return NotFound(new { status = "error", message = "Cinema not found" });
            }
            return Ok(new { status = "success", data = cinema });
        }

        [HttpGet("city/{id}")]
        public async Task<IActionResult> GetCinemasByCityId(int id)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            var cinema = await _cinemaRepository.GetCinemasByCityId(id);
            if (cinema == null)
            {
                return NotFound(new { status = "error", message = "Cinema not found" });
            }
            return Ok(new { status = "success", data = cinema });
        }
        [HttpGet("movie/{id}")]
        public async Task<IActionResult> GetCinemasByMovieId(int id)
        {
            var connection = _dbConnectionFactory.CreateConnection();
            var cinema = await _cinemaRepository.GetCinemasByMovieId(id);
            if (cinema == null)
            {
                return NotFound(new { status = "error", message = "Cinema not found" });
            }
            return Ok(new { status = "success", data = cinema });
        }
    }
}
