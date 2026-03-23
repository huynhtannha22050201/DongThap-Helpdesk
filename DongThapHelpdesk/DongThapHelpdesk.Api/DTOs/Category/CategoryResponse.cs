namespace DongThapHelpdesk.Api.DTOs.Category;

public class CategoryResponse
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    // Tên danh mục cha để hiển thị trên UI
    public int DefaultSlaHours { get; set; }
    public bool IsActive { get; set; }
    public List<CategoryResponse> Children { get; set; } = new();
    // Danh sách danh mục con
}