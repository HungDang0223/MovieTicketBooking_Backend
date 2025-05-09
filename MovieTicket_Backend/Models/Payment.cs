namespace MovieTicket_Backend.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int BookingId { get; set; }

        public string TransactionId { get; set; } = string.Empty;
        public string BankCode { get; set; } = string.Empty;
        public string TransactionStatus { get; set; } = string.Empty;
        public string ResponseCode { get; set; } = string.Empty;
        public string Amount { get; set; } = string.Empty;
        public string OrderInfo { get; set; } = string.Empty;

    }

}
