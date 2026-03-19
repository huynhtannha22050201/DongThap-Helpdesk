using DongThapHelpdesk.Api.Constants;
using DongThapHelpdesk.Api.Models;
using DongThapHelpdesk.Api.Services;
using DongThapHelpdesk.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DongThapHelpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TicketsController : ControllerBase
{
    private readonly TicketService _service;

    public TicketsController(TicketService service)
    {
        _service = service;
    }

    /// <summary>Lấy toàn bộ danh sách ticket</summary>
    [HttpGet]
    [Authorize(Roles = Roles.DispatcherAndManager)]
    public async Task<ActionResult<List<Ticket>>> GetAll()
    {
        var tickets = await _service.GetAllAsync();
        return Ok(tickets);
    }

    /// <summary>Lấy ticket theo MongoDB ObjectId</summary>
    [HttpGet("{id}")]
    [Authorize(Roles = Roles.StaffOnly)]
    public async Task<ActionResult<Ticket>> GetById(string id)
    {
        var ticket = await _service.GetByIdAsync(id);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    /// <summary>Lấy ticket theo mã số - công khai không cần đăng nhập</summary>
    [HttpGet("code/{code}")]
    [AllowAnonymous]
    public async Task<ActionResult<Ticket>> GetByCode(string code)
    {
        var ticket = await _service.GetByCodeAsync(code);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    /// <summary>
    /// Lấy danh sách ticket được giao cho đơn vị
    /// Chỉ Assignee mới xem được - chỉ thấy ticket của đơn vị mình
    /// </summary>
    [HttpGet("assigned")]
    [Authorize(Roles = Roles.Assignee)]
    public async Task<ActionResult<List<Ticket>>> GetAssigned()
    {
        var departmentId = User.GetDepartmentId();

        if (string.IsNullOrEmpty(departmentId))
            return BadRequest("Tài khoản chưa được gán đơn vị");

        var tickets = await _service
            .GetByDepartmentAsync(departmentId);
        return Ok(tickets);
    }
}