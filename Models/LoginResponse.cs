namespace digital_business_card.Models
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
    // JWT (legacy iki aşamalı auth yerine doğrudan kullanılabilir)
    public string AccessToken { get; set; } = string.Empty;
    }
}
