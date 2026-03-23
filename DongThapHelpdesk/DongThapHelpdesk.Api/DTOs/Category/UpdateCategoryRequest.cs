namespace DongThapHelpdesk.Api.DTOs.Category;

public class UpdateCategoryRequest
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public int DefaultSlaHours { get; set; }
    public bool IsActive { get; set; }
}