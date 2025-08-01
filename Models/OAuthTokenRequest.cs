namespace digital_business_card.Models
{
    public class OAuthTokenRequest
    {
        public string RequestId { get; set; } = string.Empty;
        public string AuthToken { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
    }
}
