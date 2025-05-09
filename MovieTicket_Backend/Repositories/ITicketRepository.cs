using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.Repositories
{
    public interface ITicketRepository
    {

        Task<List<Ticket>> GetTicketsByUserId(string userId);
    }
}
