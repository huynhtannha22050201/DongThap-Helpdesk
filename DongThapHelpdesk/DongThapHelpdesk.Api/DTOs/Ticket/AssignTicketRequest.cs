namespace DongThapHelpdesk.Api.DTOs.Ticket;

public class AssignTicketRequest
{
    public string DepartmentId { get; set; } = null!;
    // ID đơn vị được giao

    public string? AssignedUserId { get; set; }
    // ID cán bộ cụ thể (có thể null)

    public string? Note { get; set; }
    // Ghi chú của Dispatcher khi giao việc
}