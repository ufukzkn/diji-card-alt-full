using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace diji_card_alt.Models
{
    public class UserDefinitionValue
    {
        [Key]
        public int Id { get; set; } // Auto-increment primary key

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int DefinitionId { get; set; }

        [Required]
        public string Value { get; set; } = string.Empty;

        public int SortId { get; set; } // Sıralama için eklendi

        // Custom Definition Name (nullable) - sadece custom tanımlar için
        public string? CustomDefinitionName { get; set; }

        // Frontend için gösterilecek isim - artık Definition navigation property olmadığı için bu computed'ı kaldırdık
        // DisplayName'i controller'da query ile alıyoruz

        // Helper property
        [NotMapped]
        public bool IsCustom => DefinitionId == 11;
    }
}
