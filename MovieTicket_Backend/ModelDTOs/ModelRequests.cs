namespace MovieTicket_Backend.ModelDTOs
{
    public class ModelRequests
    {
        public class BookingRequestDTO
        {
            public string UserId { get; set; }
            public int ShowingId { get; set; }
            public List<int> SeatIds { get; set; } = new List<int>();
            public List<SnackRequestDTO> Snacks { get; set; } = new List<SnackRequestDTO>();
            public List<ComboRequestDTO> Combos { get; set; } = new List<ComboRequestDTO>();
            public DateTime BookingDate { get; set; }
            public decimal BookingAmount { get; set; }
            public int? DiscountId { get; set; }
        }

        public class SnackRequestDTO
        {
            public int SnackId { get; set; }
            public int Quantity { get; set; }
        }

        public class ComboRequestDTO
        {
            public int ComboId { get; set; }
            public int Quantity { get; set; }
        }

        public class UpdatePasswordRequest
        {
            public string emailPhone { get; set; } = string.Empty;
            public string newPassword { get; set; } = string.Empty;
        }

        public class EmailRequest
        {
            public string To { get; set; } = string.Empty;
        }
    }
}
