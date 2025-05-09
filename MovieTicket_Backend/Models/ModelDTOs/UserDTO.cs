namespace MovieTicket_Backend.Models.ModelDTOs
{
    public class UserDTO
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = "Nam";
        public string Address { get; set; } = string.Empty;
        public string RankName { get; set; } = "Unrank";
        public int TotalPoint { get; set; } = 0;
        public int TotalPaid { get; set; } = 0;

    }
}
