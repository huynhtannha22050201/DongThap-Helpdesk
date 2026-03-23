namespace DongThapHelpdesk.Api.DTOs.User;

public class UpdateUserRequest
{
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? DepartmentId { get; set; }
    public bool IsActive { get; set; }
}