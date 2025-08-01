namespace digital_business_card.Models
{
    public class ValidateTokenRequest
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RequestedUserId { get; set; } = string.Empty;
    }
}
