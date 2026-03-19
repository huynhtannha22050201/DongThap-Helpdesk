namespace DongThapHelpdesk.Api.DTOs.Auth;

public class UserInfo
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    // Hiển thị SĐT trên giao diện profile
    public string Role { get; set; } = null!;
    public string? DepartmentId { get; set; }
}