using System.ComponentModel.DataAnnotations;

namespace DigitalBusinessCard.Models
{
    public class PrivateProfileAccess
    {
        [Key]
        public int Id { get; set; }
        
        public string UserId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty; // Özel link için encrypted token
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiryDate { get; set; } // Opsiyonel, link süre sınırı
        public bool IsActive { get; set; } = true;
        public string? Description { get; set; } // "Etkinlik için özel link" vb.
    }

    public class PrivateProfileAccessRequest
    {
        public string AuthCode { get; set; } = string.Empty;
        public string? Password { get; set; }
        public string? AccessToken { get; set; }
    }

    public class PrivateProfileResponse
    {
        public bool IsPublic { get; set; }
        public bool AccessGranted { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? ProfileData { get; set; }
    }

    public class CreateSpecialLinkRequest
    {
        public DateTime? ExpiryDate { get; set; }
        public string? Description { get; set; }
    }

    public class UpdatePrivacySettingsRequest
    {
        public bool IsPublic { get; set; }
        public string? PrivateAccessPassword { get; set; }
    }
}
