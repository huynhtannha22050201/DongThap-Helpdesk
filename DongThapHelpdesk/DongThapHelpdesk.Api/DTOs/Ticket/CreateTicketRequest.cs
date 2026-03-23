namespace DongThapHelpdesk.Api.DTOs.Ticket;

public class CreateTicketRequest
{
    // ── Thông tin báo cáo ─────────────────────────────────
    public string Title { get; set; } = null!;
    // Tiêu đề ngắn gọn
    // Ví dụ: "Ổ gà lớn trên đường Nguyễn Huệ"

    public string Description { get; set; } = null!;
    // Mô tả chi tiết sự cố

    public string CategoryId { get; set; } = null!;
    // ID danh mục sự cố

    public double? Longitude { get; set; }
    // Kinh độ GPS

    public double? Latitude { get; set; }
    // Vĩ độ GPS

    public string? Address { get; set; }
    // Địa chỉ văn bản

    // ── Thông tin người báo cáo ───────────────────────────
    public string ReporterName { get; set; } = null!;
    // Họ tên người báo cáo
    // Ví dụ: "Nguyễn Văn An"

    public string ReporterPhone { get; set; } = null!;
    // Số điện thoại người báo cáo
    // Dùng để tạo tài khoản sau khi duyệt

    public string? ReporterAddress { get; set; }
    // Địa chỉ thường trú
    // Ví dụ: "123 Nguyễn Huệ, Phường 1, TP. Cao Lãnh"

    public string? ReporterWard { get; set; }
    // Phường/Xã

    public string? ReporterDistrict { get; set; }
    // Quận/Huyện
    public List<IFormFile>? Files { get; set; }
    // Ảnh/video đính kèm từ camera hoặc thư viện ảnh
    // Không bắt buộc - người dân có thể gửi không kèm ảnh
}