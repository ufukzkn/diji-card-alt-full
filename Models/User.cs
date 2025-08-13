using System.ComponentModel.DataAnnotations;

namespace diji_card_alt.Models
{
    public class User
    {
        [Key]
        public string UserId { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string JobTitle { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;

        // Authentication
        public string Password { get; set; } = string.Empty;

        // Profile photo properties
        public string? ProfilePhotoUrl { get; set; }

    // Profil herkese açık mı? (Arama / görüntüleme izni)
    public bool IsPublic { get; set; } = true;
    }
}
