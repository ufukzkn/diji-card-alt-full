namespace diji_card_alt.Models
{
    public class AddCustomDefinitionRequest
    {
        public string UserId { get; set; } = string.Empty;
        public string CustomDefinitionName { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
        public int SortId { get; set; }
    }
}
