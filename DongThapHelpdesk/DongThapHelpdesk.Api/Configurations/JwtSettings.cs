namespace DongThapHelpdesk.Api.Configurations
{
    public class JwtSettings
    {
        public string SecretKey { get; set; } = null!;
        // Khóa bí mật để ký và xác thực Token
        // Phải đủ dài và phức tạp, không được lộ ra ngoài

        public string Issuer { get; set; } = null!;
        // Tên hệ thống phát hành Token
        // Ví dụ: "DongThapHelpdesk.Api"

        public string Audience { get; set; } = null!;
        // Đối tượng sử dụng Token
        // Ví dụ: "DongThapHelpdesk.Client"

        public int ExpiryHours { get; set; } = 8;
        // Thời gian Token hết hạn tính bằng giờ
        // Ví dụ: 8 = Token hết hạn sau 8 giờ làm việc
    }
}
