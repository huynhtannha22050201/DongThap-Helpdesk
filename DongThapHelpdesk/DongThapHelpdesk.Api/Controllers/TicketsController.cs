using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DongThapHelpdesk.Api.Constants;
using DongThapHelpdesk.Api.DTOs.Ticket;
using DongThapHelpdesk.Api.Extensions;
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

    // ══════════════════════════════════════════════════════
    // GET
    // ══════════════════════════════════════════════════════

    [HttpGet]
    [Authorize(Roles = Roles.DispatcherAndManager)]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    [Authorize(Roles = Roles.StaffOnly)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null
            ? NotFound(new
            {
                message = "Không tìm thấy phản ánh"
            })
            : Ok(result);
    }

    [HttpGet("code/{code}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _service.GetByCodeAsync(code);
        return result == null
            ? NotFound(new
            {
                message = "Không tìm thấy phản ánh"
            })
            : Ok(result);
    }

    [HttpGet("assigned")]
    [Authorize(Roles = Roles.Assignee)]
    public async Task<IActionResult> GetAssigned()
    {
        var departmentId = User.GetDepartmentId();
        if (string.IsNullOrEmpty(departmentId))
            return BadRequest(new
            {
                message = "Tài khoản chưa được gán đơn vị"
            });

        return Ok(await _service
            .GetByDepartmentAsync(departmentId));
    }

    [HttpGet("{id}/activities")]
    [Authorize(Roles = Roles.StaffOnly)]
    public async Task<IActionResult> GetActivities(string id)
        => Ok(await _service.GetActivitiesAsync(id));

    // ══════════════════════════════════════════════════════
    // UC01: GỬI PHẢN ÁNH
    // ══════════════════════════════════════════════════════

    [HttpPost]
    [AllowAnonymous]
    [RequestSizeLimit(52428800)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create(
        [FromForm] CreateTicketRequest request)
    {
        if (string.IsNullOrEmpty(request.Title))
            return BadRequest(new
            {
                message = "Tiêu đề không được để trống"
            });

        if (string.IsNullOrEmpty(request.Description))
            return BadRequest(new
            {
                message = "Mô tả không được để trống"
            });

        if (string.IsNullOrEmpty(request.CategoryId))
            return BadRequest(new
            {
                message = "Vui lòng chọn danh mục sự cố"
            });

        if (string.IsNullOrEmpty(request.ReporterName))
            return BadRequest(new
            {
                message = "Họ tên không được để trống"
            });

        if (string.IsNullOrEmpty(request.ReporterPhone))
            return BadRequest(new
            {
                message = "Số điện thoại không được để trống"
            });

        var (success, message, data) =
            await _service.CreateAsync(request);

        return success
            ? CreatedAtAction(
                nameof(GetById),
                new { id = data!.Id },
                data)
            : BadRequest(new { message });
    }

    // ══════════════════════════════════════════════════════
    // UC05: DISPATCHER KIỂM DUYỆT
    // ══════════════════════════════════════════════════════

    [HttpPut("{id}/approve")]
    [Authorize(Roles = Roles.AdminAndDispatcher)]
    public async Task<IActionResult> Approve(
        string id,
        [FromBody] ApproveTicketRequest request)
    {
        var (success, message) = await _service.ApproveAsync(
            id, User.GetUserId(), request);

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }

    [HttpPut("{id}/reject")]
    [Authorize(Roles = Roles.AdminAndDispatcher)]
    public async Task<IActionResult> Reject(
        string id,
        [FromBody] RejectTicketRequest request)
    {
        if (string.IsNullOrEmpty(request.Reason))
            return BadRequest(new
            {
                message = "Vui lòng nhập lý do từ chối"
            });

        var (success, message) = await _service.RejectAsync(
            id, User.GetUserId(), request);

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }

    // ══════════════════════════════════════════════════════
    // UC06: DISPATCHER GIAO VIỆC
    // ══════════════════════════════════════════════════════

    [HttpPut("{id}/assign")]
    [Authorize(Roles = Roles.AdminAndDispatcher)]
    public async Task<IActionResult> Assign(
        string id,
        [FromBody] AssignTicketRequest request)
    {
        if (string.IsNullOrEmpty(request.DepartmentId))
            return BadRequest(new
            {
                message = "Vui lòng chọn đơn vị xử lý"
            });

        var (success, message) = await _service.AssignAsync(
            id, User.GetUserId(), request);

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }

    // ══════════════════════════════════════════════════════
    // UC09: ASSIGNEE TIẾP NHẬN
    // ══════════════════════════════════════════════════════

    [HttpPut("{id}/inprogress")]
    [Authorize(Roles = Roles.Assignee)]
    public async Task<IActionResult> StartProgress(string id)
    {
        var (success, message) = await _service
            .StartProgressAsync(id, User.GetUserId());

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }

    // ══════════════════════════════════════════════════════
    // UC10: ASSIGNEE BÁO CÁO KẾT QUẢ
    // ══════════════════════════════════════════════════════

    [HttpPut("{id}/result")]
    [Authorize(Roles = Roles.Assignee)]
    [RequestSizeLimit(52428800)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitResult(
        string id,
        [FromForm] SubmitResultRequest request)
    {
        if (string.IsNullOrEmpty(request.ResultNote))
            return BadRequest(new
            {
                message = "Vui lòng nhập nội dung kết quả"
            });

        var (success, message) = await _service
            .SubmitResultAsync(
                id,
                User.GetUserId(),
                request);

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }

    // ══════════════════════════════════════════════════════
    // UC11: ASSIGNEE CHUYỂN TRẢ
    // ══════════════════════════════════════════════════════

    [HttpPut("{id}/reassign")]
    [Authorize(Roles = Roles.Assignee)]
    public async Task<IActionResult> Reassign(
        string id,
        [FromBody] ReassignTicketRequest request)
    {
        if (string.IsNullOrEmpty(request.Reason))
            return BadRequest(new
            {
                message = "Vui lòng nhập lý do chuyển trả"
            });

        var (success, message) = await _service
            .ReassignAsync(
                id,
                User.GetUserId(),
                request);

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }

    // ══════════════════════════════════════════════════════
    // UC07: DISPATCHER ĐÓNG PHẢN ÁNH
    // ══════════════════════════════════════════════════════

    [HttpPut("{id}/close")]
    [Authorize(Roles = Roles.AdminAndDispatcher)]
    public async Task<IActionResult> Close(string id)
    {
        var (success, message) = await _service
            .CloseAsync(id, User.GetUserId());

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }

    // ══════════════════════════════════════════════════════
    // UC03: CITIZEN ĐÁNH GIÁ
    // ══════════════════════════════════════════════════════

    [HttpPost("{id}/rating")]
    [Authorize(Roles = Roles.Citizen)]
    public async Task<IActionResult> Rate(
        string id,
        [FromBody] RatingRequest request)
    {
        if (request.Stars < 1 || request.Stars > 5)
            return BadRequest(new
            {
                message = "Số sao phải từ 1 đến 5"
            });

        var (success, message) = await _service.RateAsync(
            id, User.GetUserId(), request);

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }
}