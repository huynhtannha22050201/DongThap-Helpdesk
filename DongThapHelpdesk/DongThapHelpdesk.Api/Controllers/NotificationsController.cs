using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DongThapHelpdesk.Api.Extensions;
using DongThapHelpdesk.Api.Services;

namespace DongThapHelpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly NotificationService _service;

    public NotificationsController(
        NotificationService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy tất cả thông báo của user hiện tại
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service
            .GetByUserAsync(User.GetUserId()));

    /// <summary>
    /// Đếm số thông báo chưa đọc
    /// Dùng để hiển thị badge đỏ trên UI
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _service
            .CountUnreadAsync(User.GetUserId());
        return Ok(new { count });
    }

    /// <summary>
    /// Đánh dấu một thông báo đã đọc
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(string id)
    {
        var (success, message) = await _service
            .MarkAsReadAsync(id, User.GetUserId());

        return success
            ? Ok(new { message })
            : NotFound(new { message });
    }

    /// <summary>
    /// Đánh dấu tất cả thông báo đã đọc
    /// </summary>
    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllAsRead()
    {
        await _service.MarkAllAsReadAsync(
            User.GetUserId());
        return Ok(new
        {
            message = "Đã đánh dấu tất cả đã đọc"
        });
    }
}