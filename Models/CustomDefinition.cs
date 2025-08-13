using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace diji_card_alt.Models
{
    public class CustomDefinition
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public string DefinitionName { get; set; } = string.Empty;

        public string? Value { get; set; }

        public int SortId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonIgnore]
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
    }
}
