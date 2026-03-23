namespace DongThapHelpdesk.Api.DTOs.User;

public class UserResponse
{
    public string Id { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PhoneNumber { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    // Tên đơn vị để hiển thị trên UI
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}