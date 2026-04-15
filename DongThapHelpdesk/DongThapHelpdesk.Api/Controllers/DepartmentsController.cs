using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DongThapHelpdesk.Api.Constants;
using DongThapHelpdesk.Api.DTOs.Department;
using DongThapHelpdesk.Api.Services;

namespace DongThapHelpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController : ControllerBase
{
    private readonly DepartmentService _service;

    public DepartmentsController(DepartmentService service)
    {
        _service = service;
    }

    /// <summary>
    /// Lấy danh sách đơn vị
    /// Tất cả Staff đều xem được
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.StaffOnly)]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    /// <summary>Lấy chi tiết một đơn vị</summary>
    [HttpGet("{id}")]
    [Authorize(Roles = Roles.StaffOnly)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        if (result == null) return NotFound(
            new { message = "Không tìm thấy đơn vị" });
        return Ok(result);
    }

    /// <summary>
    /// Tạo đơn vị mới
    /// Chỉ Admin
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepartmentRequest request)
    {
        if (string.IsNullOrEmpty(request.Name) ||
            string.IsNullOrEmpty(request.Code))
            return BadRequest(
                new { message = "Tên và mã không được để trống" });

        var (success, message, data) =
            await _service.CreateAsync(request);

        if (!success)
            return BadRequest(new { message });

        return CreatedAtAction(
            nameof(GetById),
            new { id = data!.Id },
            data);
    }

    /// <summary>
    /// Lấy danh sách phòng ban có phân trang + lọc + thống kê
    /// </summary>
    [HttpGet("paged")]
    [Authorize(Roles = Roles.StaffOnly)]
    public async Task<IActionResult> GetPaged(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 5,
    [FromQuery] string? search = null,
    [FromQuery] bool? isActive = null,
    [FromQuery] string? sortBy = null,
    [FromQuery] string? sortDir = "desc")
    {
        var result = await _service.GetPagedAsync(
            page, pageSize, search, isActive, sortBy, sortDir);
        return Ok(result);
    }

    /// <summary>
    /// Cập nhật đơn vị
    /// Chỉ Admin
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateDepartmentRequest request)
    {
        var (success, message) =
            await _service.UpdateAsync(id, request);

        if (!success)
            return NotFound(new { message });

        return Ok(new { message });
    }

    /// <summary>
    /// Xóa mềm đơn vị
    /// Chỉ Admin
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Delete(string id)
    {
        var (success, message) =
            await _service.DeleteAsync(id);

        if (!success)
            return NotFound(new { message });

        return Ok(new { message });
    }
}