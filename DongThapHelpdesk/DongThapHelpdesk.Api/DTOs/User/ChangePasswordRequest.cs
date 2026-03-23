namespace DongThapHelpdesk.Api.DTOs.User;

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = null!;
    // Mật khẩu hiện tại

    public string NewPassword { get; set; } = null!;
    // Mật khẩu mới

    public string ConfirmPassword { get; set; } = null!;
    // Xác nhận mật khẩu mới
}