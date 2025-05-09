using Dapper;
using MovieTicket_Backend.Data;
using MovieTicket_Backend.Models;
using MovieTicket_Backend.Models.ModelDTOs;
using MovieTicket_Backend.Repositories;

namespace MovieTicket_Backend.RepositoryInpl
{
    public class BookingRepository : IBookingRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly IConfiguration _configuration;
        public BookingRepository(DbConnectionFactory dbConnectionFactory, IConfiguration configuration)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _configuration = configuration;
        }

        public async Task<string?> CreateBooking(BookingRequestDTO bookingRequest)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            await connection.OpenAsync();
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Insert booking details
                string bookingQuery = @"INSERT INTO ticket (user_id, showing_id, booking_date, booking_amount, discount_id)
                                VALUES (@UserId, @ShowingId, @BookingDate, @BookingAmount, @DiscountId);
                                SELECT LAST_INSERT_ID();";
                var bookingId = await connection.ExecuteScalarAsync<string>(bookingQuery, new
                {
                    bookingRequest.UserId,
                    bookingRequest.ShowingId,
                    bookingRequest.BookingDate,
                    bookingRequest.BookingAmount,
                    bookingRequest.DiscountId
                }, transaction);

                // Insert seats
                string seatQuery = @"INSERT INTO booking_seat (booking_id, seat_id) VALUES (@BookingId, @SeatId);";
                foreach (var seatId in bookingRequest.SeatIds)
                {
                    await connection.ExecuteAsync(seatQuery, new { BookingId = bookingId, SeatId = seatId }, transaction);
                }

                // Insert snacks
                string snackQuery = @"INSERT INTO booking_snack (booking_id, snack_id, quantity) VALUES (@BookingId, @SnackId, @Quantity);";
                foreach (var snack in bookingRequest.Snacks)
                {
                    await connection.ExecuteAsync(snackQuery, new { BookingId = bookingId, SnackId = snack.SnackId, snack.Quantity }, transaction);
                }

                // Insert combos
                string comboQuery = @"INSERT INTO booking_combo (booking_id, combo_id, quantity) VALUES (@BookingId, @ComboId, @Quantity);";
                foreach (var combo in bookingRequest.Combos)
                {
                    await connection.ExecuteAsync(comboQuery, new { BookingId = bookingId, ComboId = combo.ComboId, combo.Quantity }, transaction);
                }

                await transaction.CommitAsync();
                return bookingId;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                Console.WriteLine($"Error creating booking: {ex.Message}");
                return null;
            }
        }
        public async Task<BookingDetailDTO?> GetBookingDetails(int bookingId)
        {
            using var connection = _dbConnectionFactory.CreateConnection();

            // Query to get movie and screen details
            string movieQuery = @"
                SELECT 
                    m.title AS Title,
                    s.screen_name AS ScreenName
                FROM ticket b
                JOIN showing_movie sm ON b.showing_id = sm.showing_id
                JOIN movie m ON sm.movie_id = m.movie_id
                JOIN screen s ON sm.screen_id = s.screen_id
                WHERE b.booking_id = @BookingId";

            // Query to get seat names
            string seatQuery = @"
                SELECT 
                    CONCAT(sr.row_name, s.seat_number) AS SeatName
                FROM booking_seat bs
                JOIN seat s ON bs.seat_id = s.seat_id
                JOIN screen_row sr ON s.row_id = sr.row_id
                WHERE bs.booking_id = @BookingId";

            // Query to get snacks
            string snackQuery = @"
                SELECT 
                    sn.snack_name AS SnackName,
                    bs.quantity AS Quantity
                FROM booking_snack bs
                JOIN snack sn ON bs.snack_id = sn.snack_id
                WHERE bs.booking_id = @BookingId";

            // Query to get combos
            string comboQuery = @"
                SELECT 
                    c.combo_name AS ComboName,
                    bc.quantity AS Quantity
                FROM booking_combo bc
                JOIN combo c ON bc.combo_id = c.combo_id
                WHERE bc.booking_id = @BookingId";

            // Execute queries
            var movieDetails = await connection.QueryFirstOrDefaultAsync<dynamic>(movieQuery, new { BookingId = bookingId });
            if (movieDetails == null)
            {
                return null; // Booking not found
            }

            var seatNames = (await connection.QueryAsync<string>(seatQuery, new { BookingId = bookingId })).ToList();
            var snacks = (await connection.QueryAsync<SnackDTO>(snackQuery, new { BookingId = bookingId })).ToList();
            var combos = (await connection.QueryAsync<ComboDTO>(comboQuery, new { BookingId = bookingId })).ToList();

            // Construct the response
            return new BookingDetailDTO
            {
                Title = movieDetails.Title,
                ScreenName = movieDetails.ScreenName, // Dynamically retrieved from the query
                SeatNames = seatNames,
                Snacks = snacks,
                Combos = combos
            };
        }


    }
}
