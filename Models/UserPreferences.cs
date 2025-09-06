using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DigitalBusinessCard.Models
{
    [Table("UserPreferences")]
    public class UserPreferences
    {
        [Key]
        public string UserId { get; set; } = string.Empty;

        // Display preferences        
        [Range(3, 5)]
        public int GridColumns { get; set; } = 3;
        
        [StringLength(10)]
        public string ViewMode { get; set; } = "list"; // "list" or "grid"

        // Future theme/design preferences
        [StringLength(20)]
        public string? ThemeColor { get; set; } = "orange";
        
        [StringLength(30)]
        public string? FontFamily { get; set; } = "Inter"; // "Inter", "Roboto", "Poppins", "Open Sans"
    }
}
