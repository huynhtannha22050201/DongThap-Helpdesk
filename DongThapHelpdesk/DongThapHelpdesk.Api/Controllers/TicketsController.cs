using Microsoft.AspNetCore.Mvc;
using DongThapHelpdesk.Api.Services;

namespace DongThapHelpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly TicketService _service;

    public TicketsController(TicketService service)
    {
        _service = service;
    }

    /// <summary>Lấy toàn bộ danh sách ticket</summary>
    [HttpGet]
    public async Task<ActionResult<List<Ticket>>> GetAll()
    {
        var tickets = await _service.GetAllAsync();
        return Ok(tickets);
    }

    /// <summary>Lấy ticket theo MongoDB ObjectId</summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Ticket>> GetById(string id)
    {
        var ticket = await _service.GetByIdAsync(id);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }

    /// <summary>Lấy ticket theo mã số (VD: PA-032026-001)</summary>
    [HttpGet("code/{code}")]
    public async Task<ActionResult<Ticket>> GetByCode(string code)
    {
        var ticket = await _service.GetByCodeAsync(code);
        if (ticket == null) return NotFound();
        return Ok(ticket);
    }
}