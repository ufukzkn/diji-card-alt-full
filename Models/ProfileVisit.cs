using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace diji_card_alt.Models
{
    public class ProfileVisit
    {
        [Key]
        public long Id { get; set; }

        [Required]
        public string ProfileUserId { get; set; } = string.Empty; // Ziyaret edilen profil

        public string? VisitorUserId { get; set; } // Giriş yapmış kullanıcı (anonimse null)

        [Required]
        public DateTime VisitedAtUtc { get; set; } = DateTime.UtcNow;

        public string? VisitorIpHash { get; set; } // IP hash (kişisel veri yerine)

        public string? UserAgentHash { get; set; } // UA hash (isteğe bağlı analiz)

        [ForeignKey(nameof(ProfileUserId))]
        public User? ProfileUser { get; set; }

        [ForeignKey(nameof(VisitorUserId))]
        public User? VisitorUser { get; set; }
    }
}
