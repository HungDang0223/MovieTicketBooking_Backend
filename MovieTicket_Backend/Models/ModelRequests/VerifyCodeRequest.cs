namespace MovieTicket_Backend.Models.ModelRequests
{
    public class VerifyCodeRequest
    {
        public string EmailOrPhone { get; set; }
        public string Code { get; set; }
        public VerifyCodeRequest(string emailOrPhone, string code)
        {
            EmailOrPhone = emailOrPhone;
            Code = code;
        }
    }
}
