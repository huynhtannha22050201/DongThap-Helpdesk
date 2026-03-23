namespace DongThapHelpdesk.Api.DTOs.Ticket;

public class RejectTicketRequest
{
    public string Reason { get; set; } = null!;
    // Lý do từ chối
    // Ví dụ: "Không có hình ảnh minh chứng"
}