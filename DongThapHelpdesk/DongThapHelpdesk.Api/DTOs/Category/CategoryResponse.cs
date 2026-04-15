namespace DongThapHelpdesk.Api.DTOs.Category;

public class CategoryResponse
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public int DefaultSlaHours { get; set; }
    public bool IsActive { get; set; }

    public string? Description {  get; set; }
}