using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DongThapHelpdesk.Api.Constants;
using DongThapHelpdesk.Api.DTOs.User;
using DongThapHelpdesk.Api.Extensions;
using DongThapHelpdesk.Api.Services;

namespace DongThapHelpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly UserService _service;

    public UsersController(UserService service)
    {
        _service = service;
    }

    // ── Lấy danh sách Staff ───────────────────────────────

    /// <summary>
    /// Lấy toàn bộ danh sách cán bộ
    /// Chỉ Admin
    /// </summary>
    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllStaffAsync());

    /// <summary>
    /// Lấy danh sách theo Role
    /// Dispatcher cần xem danh sách Assignee để giao việc
    /// </summary>
    [HttpGet("role/{role}")]
    [Authorize(Roles = Roles.AdminAndDispatcher)]
    public async Task<IActionResult> GetByRole(string role)
        => Ok(await _service.GetByRoleAsync(role));

    /// <summary>Lấy chi tiết một tài khoản</summary>
    [HttpGet("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await _service.GetByIdAsync(id);
        return result == null
            ? NotFound(new
            {
                message = "Không tìm thấy tài khoản"
            })
            : Ok(result);
    }

    // ── Tạo tài khoản cán bộ ─────────────────────────────

    /// <summary>
    /// Tạo tài khoản cán bộ mới
    /// Chỉ Admin
    /// </summary>
    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Create(
        [FromBody] CreateUserRequest request)
    {
        if (string.IsNullOrEmpty(request.FullName) ||
            string.IsNullOrEmpty(request.PhoneNumber) ||
            string.IsNullOrEmpty(request.Password) ||
            string.IsNullOrEmpty(request.Role))
            return BadRequest(new
            {
                message = "Vui lòng điền đầy đủ thông tin"
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

    // ── Cập nhật thông tin ────────────────────────────────

    /// <summary>
    /// Cập nhật thông tin cán bộ
    /// Chỉ Admin
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] UpdateUserRequest request)
    {
        var (success, message) =
            await _service.UpdateAsync(id, request);

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }

    // ── Khóa/Mở khóa tài khoản ───────────────────────────

    /// <summary>
    /// Khóa hoặc mở khóa tài khoản
    /// Chỉ Admin
    /// </summary>
    [HttpPut("{id}/lock")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> ToggleLock(string id)
    {
        var (success, message) =
            await _service.ToggleLockAsync(id);

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }

    // ── UC19: Đổi mật khẩu ───────────────────────────────

    /// <summary>
    /// Đổi mật khẩu của chính mình
    /// Tất cả user đã đăng nhập đều dùng được
    /// </summary>
    [HttpPut("change-password")]
    public async Task<IActionResult> ChangePassword(
        [FromBody] ChangePasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.CurrentPassword) ||
            string.IsNullOrEmpty(request.NewPassword) ||
            string.IsNullOrEmpty(request.ConfirmPassword))
            return BadRequest(new
            {
                message = "Vui lòng điền đầy đủ thông tin"
            });

        var (success, message) =
            await _service.ChangePasswordAsync(
                User.GetUserId(), request);

        return success
            ? Ok(new { message })
            : BadRequest(new { message });
    }
}