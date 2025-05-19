using Dapper;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.RepositoryImpl
{
    public class ScreenRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly ApplicationDbContext _dbContext;
        public ScreenRepository(DbConnectionFactory dbConnectionFactory, ApplicationDbContext context)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _dbContext = context;
        }
        public async Task<List<Dictionary<string, dynamic>>> GetScreenSeats(int screenId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            string query = @"
                SELECT 
                    sr.row_name AS RowName,
                    sr.seat_count AS SeatCount,
                    sr.seat_type AS SeatType
                FROM screen s
                JOIN screen_row sr ON s.screen_id = sr.screen_id
                WHERE s.screen_id = @ScreenId";
            var parameters = new { ScreenId = screenId };
            var rows = await connection.QueryAsync(query, parameters);
            var result = new List<Dictionary<string, dynamic>>();
            foreach (var row in rows)
            {
                var rowDict = new Dictionary<string, dynamic>
                {
                    { row.RowName, row.SeatCount },
                    { "SeatType", row.SeatType }
                };
                result.Add(rowDict);
            }
            return result;
        }
    }
}
