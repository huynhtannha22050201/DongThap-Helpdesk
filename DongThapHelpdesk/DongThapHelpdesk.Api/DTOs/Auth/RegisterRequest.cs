namespace DongThapHelpdesk.Api.DTOs.Auth;

public class RegisterRequest
{
    public string FullName { get; set; } = null!;
    // Họ tên đầy đủ
    // Ví dụ: "Nguyễn Văn An"

    public string PhoneNumber { get; set; } = null!;
    // Số điện thoại đăng nhập
    // Ví dụ: "0901234567"

    public string Password { get; set; } = null!;
    // Mật khẩu

    public string ConfirmPassword { get; set; } = null!;
    // Xác nhận mật khẩu

    public string? Ward { get; set; }
    // Phường/Xã
    // Ví dụ: "Phường 1"

    public string? District { get; set; }
    // Quận/Huyện
    // Ví dụ: "TP. Cao Lãnh"

    public string? Address { get; set; }
    // Địa chỉ cụ thể
    // Ví dụ: "123 Nguyễn Huệ"
}