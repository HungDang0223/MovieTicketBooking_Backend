using Dapper;
using Microsoft.EntityFrameworkCore;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.ModelDTOs;
using MovieTicket_Backend.Models;

namespace MovieTicket_Backend.RepositoryImpl
{
    public class ScreenRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly ApplicationDbContext _context;
        public ScreenRepository(DbConnectionFactory dbConnectionFactory, ApplicationDbContext context)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _context = context;
        }
        public async Task<List<RowSeatsDto>> GetSeatsByScreenIdAsync(int screenId)
        {
            var rowsWithSeats = await _context.ScreenRows
                .Where(sr => sr.ScreenId == screenId)
                .Select(sr => new RowSeatsDto
                {
                    RowName = sr.RowName,
                    SeatType = sr.SeatType,
                    Seats = sr.Seats
                        .Select(s => new SeatDto
                        {
                            SeatId = s.SeatId,
                            SeatNumber = s.SeatNumber
                        })
                        .OrderBy(s => s.SeatNumber)
                        .ToList()
                })
                .ToListAsync();

            return rowsWithSeats;
        }
    }
}
