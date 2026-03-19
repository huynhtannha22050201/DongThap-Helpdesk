namespace DongThapHelpdesk.Api.DTOs.Auth;

public class LoginResult
{
    public bool Success { get; set; }
    // true = đăng nhập thành công

    public string Message { get; set; } = null!;
    // Thông báo trả về cho người dùng

    public string? Token { get; set; }
    // JWT Token — chỉ dùng nội bộ
    // Không trả ra ngoài vì dùng HttpOnly Cookie

    public UserInfo? User { get; set; }
    // Thông tin người dùng trả về cho Vue
}