namespace DongThapHelpdesk.Api.DTOs.Department;

public class DepartmentResponse
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Level { get; set; } = null!;
    public string? ParentId { get; set; }
    public string? ParentName { get; set; }
    // Tên đơn vị cấp trên
    public string? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    // Tên cán bộ đầu mối
    public bool IsActive { get; set; }
}