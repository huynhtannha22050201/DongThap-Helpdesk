namespace DongThapHelpdesk.Api.DTOs.Category;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? Description { get; set; }
    public int DefaultSlaHours { get; set; } = 72;
    public bool IsActive { get; set; }
}