namespace DongThapHelpdesk.Api.DTOs.Department;

public class UpdateDepartmentRequest
{
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string? ResponsibleUserId { get; set; }
    public bool IsActive { get; set; }
}