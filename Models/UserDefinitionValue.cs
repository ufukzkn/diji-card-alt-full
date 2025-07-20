using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace diji_card_alt.Models
{
    public class UserDefinitionValue
    {
        [Required]
        public string UserId { get; set; }

        [Required]
        public int DefinitionId { get; set; }

        [Required]
        public string Value { get; set; }

        public int SortId { get; set; } // Sıralama için eklendi

        [JsonIgnore]
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }


        [JsonIgnore]
        [ForeignKey(nameof(DefinitionId))]
        public Definition? Definition { get; set; }
    }
}
