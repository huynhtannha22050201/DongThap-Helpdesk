namespace DongThapHelpdesk.Api.DTOs.Department;

public class CreateDepartmentRequest
{
    public string Name { get; set; } = null!;
    // Tên đơn vị
    // Ví dụ: "UBND Phường 1"

    public string Code { get; set; } = null!;
    // Mã đơn vị
    // Ví dụ: "UBND-P1"

    public string Level { get; set; } = null!;
    // Cấp hành chính: Province, District, Commune

    public string? ParentId { get; set; }
    // ID đơn vị cấp trên

    public string? ResponsibleUserId { get; set; }
    // ID cán bộ đầu mối
}