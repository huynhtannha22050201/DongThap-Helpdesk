namespace DongThapHelpdesk.Api.DTOs.Category;

public class CreateCategoryRequest
{
    public string Name { get; set; } = null!;
    // Tên danh mục
    // Ví dụ: "Rác thải"

    public string Code { get; set; } = null!;
    // Mã viết tắt
    // Ví dụ: "MT-RAC"

    public string? ParentCategoryId { get; set; }
    // ID danh mục cha
    // Null nếu là danh mục gốc

    public int DefaultSlaHours { get; set; } = 72;
    // Số giờ xử lý mặc định
}