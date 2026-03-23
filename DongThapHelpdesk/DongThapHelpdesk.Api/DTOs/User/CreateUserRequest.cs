namespace DongThapHelpdesk.Api.DTOs.User;

public class CreateUserRequest
{
    public string FullName { get; set; } = null!;
    // Họ tên cán bộ

    public string PhoneNumber { get; set; } = null!;
    // Số điện thoại đăng nhập

    public string Email { get; set; } = null!;
    // Email công vụ

    public string Password { get; set; } = null!;
    // Mật khẩu khởi tạo

    public string Role { get; set; } = null!;
    // Vai trò: Dispatcher, Assignee, Manager

    public string? DepartmentId { get; set; }
    // ID đơn vị — bắt buộc với Assignee
}