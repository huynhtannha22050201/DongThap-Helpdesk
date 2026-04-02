namespace DongThapHelpdesk.Api.DTOs.Notification;

public class NotificationResponse
{
    public string Id { get; set; } = null!;
    public string? TicketId { get; set; }
    public string? TicketCode { get; set; }
    // Mã ticket để hiển thị trên UI
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}