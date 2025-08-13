namespace diji_card_alt.Models
{
    // Frontend için optimize edilmiş DTO
    public class UserDefinitionValueDto
    {
        public int Id { get; set; } // Auto-increment ID
        public string UserId { get; set; } = string.Empty;
        public int DefinitionId { get; set; }
        public string Value { get; set; } = string.Empty;
        public int SortId { get; set; }
        
        // Frontend için gösterilecek isim (CustomDefinitionName veya DefinitionName)
        public string DisplayName { get; set; } = string.Empty;
        
        // Custom definition name (sadece custom tanımlar için)
        public string? CustomDefinitionName { get; set; }
        
        // Definition type helper
        public bool IsCustom => DefinitionId == 11;
    }
}
