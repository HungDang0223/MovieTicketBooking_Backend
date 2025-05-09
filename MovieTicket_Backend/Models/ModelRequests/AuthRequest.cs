namespace MovieTicket_Backend.Models.ModelRequests
{
    public class RegisterRequest
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Gender { get; set; }
        public string Password { get; set; }
        public DateTime DateOfBirth { get; set; }
        public string Address { get; set; }
    }

    public class LoginRequest
    {
        public string EmailOrPhone { get; set; }
        public string Password { get; set; }
    }

}
