using Dapper;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Repositories;

namespace MovieTicket_Backend.RepositoryInpl
{
    public class TicketRepository : ITicketRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        public TicketRepository(DbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
        }

        public async Task<List<Ticket>> GetTicketsByUserId(string userId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            var query = "SELECT * FROM ticket WHERE user_id = @UserId";
            var tickets = await connection.QueryAsync<Ticket>(query, new { UserId = userId });
            return tickets.ToList();
        }
    }
}
