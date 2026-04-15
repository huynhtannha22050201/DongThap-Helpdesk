namespace DongThapHelpdesk.Api.DTOs.Category;

public class CreateCategoryRequest
{
    public string Name { get; set; } = null!;
    // Tên danh mục
    // Ví dụ: "Rác thải"

    public string Code { get; set; } = null!;
    // Mã viết tắt
    // Ví dụ: "MT-RAC"
    public int DefaultSlaHours { get; set; } = 72;
    // Số giờ xử lý mặc định

    public string? Description {  get; set; }
}