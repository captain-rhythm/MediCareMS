namespace MediCareMS.Models.Entities;

public class SystemSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = string.Empty;
    public string? Value { get; set; }
    public string Category { get; set; } = "General";
    public string? Label { get; set; }
    public bool IsSecret { get; set; }
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
