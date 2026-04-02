using MongoDB.Driver;
using DongThapHelpdesk.Api.DTOs.Notification;
using DongThapHelpdesk.Api.Enums;
using DongThapHelpdesk.Api.Models;
using DongThapHelpdesk.Api.Repositories;

namespace DongThapHelpdesk.Api.Services;

public class NotificationService
{
    private readonly NotificationRepository _repo;
    private readonly TicketRepository _ticketRepo;

    public NotificationService(
        NotificationRepository repo,
        TicketRepository ticketRepo)
    {
        _repo = repo;
        _ticketRepo = ticketRepo;
    }

    // ── Lấy thông báo của user hiện tại ──────────────────
    public async Task<List<NotificationResponse>>
        GetByUserAsync(string userId)
    {
        var filter = Builders<Notification>.Filter
            .Eq(n => n.RecipientId, userId);

        var notifications = await _repo
            .GetByFilterAsync(filter);

        return await MapToResponseListAsync(notifications);
    }

    // ── Đếm thông báo chưa đọc ───────────────────────────
    public async Task<long> CountUnreadAsync(string userId)
    {
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter
                .Eq(n => n.RecipientId, userId),
            Builders<Notification>.Filter
                .Eq(n => n.IsRead, false));

        return await _repo.CountByFilterAsync(filter);
    }

    // ── Đánh dấu một thông báo đã đọc ────────────────────
    public async Task<(bool Success, string Message)>
        MarkAsReadAsync(string notificationId, string userId)
    {
        // Nghiệp vụ: chỉ đọc thông báo của chính mình
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter
                .Eq(n => n.Id, notificationId),
            Builders<Notification>.Filter
                .Eq(n => n.RecipientId, userId));

        var notifications = await _repo
            .GetByFilterAsync(filter);

        if (!notifications.Any())
            return (false,
                "Không tìm thấy thông báo");

        var update = Builders<Notification>.Update
            .Set(n => n.IsRead, true);

        await _repo.UpdateFieldsAsync(
            notificationId, update);

        return (true, "Đã đánh dấu đã đọc");
    }

    // ── Đánh dấu tất cả đã đọc ───────────────────────────
    public async Task MarkAllAsReadAsync(string userId)
    {
        var filter = Builders<Notification>.Filter.And(
            Builders<Notification>.Filter
                .Eq(n => n.RecipientId, userId),
            Builders<Notification>.Filter
                .Eq(n => n.IsRead, false));

        var update = Builders<Notification>.Update
            .Set(n => n.IsRead, true);

        await _repo.UpdateManyFieldsAsync(filter, update);
    }

    // ── Tạo thông báo ─────────────────────────────────────
    // Được gọi từ TicketService sau mỗi thay đổi trạng thái
    public async Task CreateAsync(
        string recipientId,
        string? ticketId,
        NotificationType type,
        string message)
    {
        var notification = new Notification
        {
            RecipientId = recipientId,
            TicketId = ticketId,
            Type = type,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.CreateAsync(notification);
    }

    // ── Tạo thông báo cho nhiều người ────────────────────
    public async Task CreateForManyAsync(
        List<string> recipientIds,
        string? ticketId,
        NotificationType type,
        string message)
    {
        foreach (var recipientId in recipientIds)
        {
            await CreateAsync(
                recipientId, ticketId, type, message);
        }
    }

    // ── Private helpers ───────────────────────────────────
    private async Task<List<NotificationResponse>>
        MapToResponseListAsync(
        List<Notification> notifications)
    {
        var result = new List<NotificationResponse>();
        foreach (var n in notifications)
            result.Add(await MapToResponseAsync(n));
        return result;
    }

    private async Task<NotificationResponse>
        MapToResponseAsync(Notification n)
    {
        // Lấy ticketCode để hiển thị trên UI
        var ticket = n.TicketId != null
            ? await _ticketRepo.GetByIdAsync(n.TicketId)
            : null;

        return new NotificationResponse
        {
            Id = n.Id,
            TicketId = n.TicketId,
            TicketCode = ticket?.TicketCode,
            Type = n.Type.ToString(),
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        };
    }
}