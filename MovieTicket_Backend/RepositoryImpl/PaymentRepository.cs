using Dapper;
using MovieTicket_Backend.Data;
using MySql.Data.MySqlClient;

namespace MovieTicket_Backend.RepositoryInpl
{
    /// <summary>
    /// 
    /// </summary>
    public class PaymentRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;
        private readonly IConfiguration _configuration;
        private readonly MySqlConnection connection;

        public PaymentRepository(DbConnectionFactory dbConnectionFactory, IConfiguration configuration)
        {
            _dbConnectionFactory = dbConnectionFactory;
            _configuration = configuration;
            connection = _dbConnectionFactory.CreateConnection();
        }

        public async Task<string> InsertPayment(string paymentId, int bookingId, string transactionId, string bankCode, string transactionStatus, string responseCode, double amount, string orderInfo)
        {
            using var connection = _dbConnectionFactory.CreateConnection();
            try
            {
                string query = @"INSERT INTO payment (payment_id, booking_id, transaction_id, bank_code, payment_status, response_code, amount, order_info) 
                                 VALUES (@PaymentId, @BookingId, @TransactionId, @BankCode, @TransactionStatus, @ResponseCode, @Amount, @OrderInfo)";
                var parameters = new
                {
                    PaymentId = paymentId,
                    BookingId = bookingId,
                    TransactionId = transactionId,
                    BankCode = bankCode,
                    TransactionStatus = transactionStatus,
                    ResponseCode = responseCode,
                    Amount = amount / 100,
                    OrderInfo = orderInfo
                };
                await connection.ExecuteAsync(query, parameters);
                return "Payment inserted successfully";
            }
            catch (Exception ex)
            {
                return $"Error inserting payment: {ex.Message}";
            }
        }
    }
}
