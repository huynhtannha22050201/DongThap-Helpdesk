namespace DongThapHelpdesk.Api.DTOs.Ticket;

public class SubmitResultRequest
{
    public string ResultNote { get; set; } = null!;
    // Mô tả kết quả xử lý
    // Ví dụ: "Đã vá ổ gà, hoàn thành lúc 14:00"

    public List<IFormFile>? Files { get; set; } = new();
    // Danh sách URL ảnh minh chứng
}