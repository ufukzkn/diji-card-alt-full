namespace digital_business_card.Models
{
    public class LoginRequest
    {
        public string KullaniciAdi { get; set; } = string.Empty;
        public string Sifre { get; set; } = string.Empty;
        public string AuthCode { get; set; } = string.Empty;
        public string ApplicationId { get; set; } = string.Empty;
        public string RedirectUrl { get; set; } = string.Empty;
        public string RequestId { get; set; } = string.Empty;
        public string Source { get; set; } = "web";
        public bool IsShowExceptionMessage { get; set; } = false;
    }
}
