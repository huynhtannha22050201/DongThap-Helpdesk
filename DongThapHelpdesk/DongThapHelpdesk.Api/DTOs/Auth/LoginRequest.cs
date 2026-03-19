namespace DongThapHelpdesk.Api.DTOs.Auth
{
    public class LoginRequest
    {
        public string PhoneNumber { get; set; } = null!;
        // Số điện thoại đăng nhập
        // Ví dụ: "0901234567"

        public string Password { get; set; } = null!;
        // Mật khẩu người dùng nhập
    }
}
