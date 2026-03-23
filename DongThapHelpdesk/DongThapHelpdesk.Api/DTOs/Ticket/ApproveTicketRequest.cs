using DongThapHelpdesk.Api.Enums;

namespace DongThapHelpdesk.Api.DTOs.Ticket;

public class ApproveTicketRequest
{
    public TicketPriority Priority { get; set; }
        = TicketPriority.Normal;
    // Dispatcher đặt mức độ ưu tiên khi duyệt
}