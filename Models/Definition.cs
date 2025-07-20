using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace diji_card_alt.Models
{
    public class Definition
    {
        [Key]
        public int DefinitionId { get; set; }

        [Required]
        public string DefinitionName { get; set; }

        [JsonIgnore]
        public List<UserDefinitionValue>? UserDefinitionValues { get; set; }

    }
}
