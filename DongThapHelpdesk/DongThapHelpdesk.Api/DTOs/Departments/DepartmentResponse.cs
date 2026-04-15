namespace DongThapHelpdesk.Api.DTOs.Department;

public class DepartmentResponse
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Code { get; set; } = null!;
    public string Level { get; set; } = null!;
    public string? ParentId { get; set; }
    public string? ParentName { get; set; }
    public string? ResponsibleUserId { get; set; }
    public string? ResponsibleUserName { get; set; }
    public string? ResponsibleUserEmail { get; set; }
    public string? ResponsibleUserPhone { get; set; }
    public long TicketCount { get; set; }
    public long StaffCount { get; set; }
    public bool IsActive { get; set; }
}