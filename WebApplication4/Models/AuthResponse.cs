namespace WebApplication4.Models
{
    public class AuthResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
        public string RequestId { get; set; }
    }
}
