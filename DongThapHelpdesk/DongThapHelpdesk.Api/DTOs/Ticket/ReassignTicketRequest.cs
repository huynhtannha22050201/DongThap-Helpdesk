namespace DongThapHelpdesk.Api.DTOs.Ticket
{
    public class ReassignTicketRequest
    {
        public string Reason { get; set; } = null!;
        // Lý do chuyển trả
        // Ví dụ: "Không thuộc thẩm quyền của đơn vị"
    }
}
