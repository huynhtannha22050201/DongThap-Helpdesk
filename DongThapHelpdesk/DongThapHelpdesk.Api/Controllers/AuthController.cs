using DongThapHelpdesk.Api.DTOs.Auth;
using DongThapHelpdesk.Api.Services;
using DongThapHelpdesk.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DongThapHelpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
    }

    /// <summary>Đăng nhập bằng số điện thoại</summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.PhoneNumber) ||
            string.IsNullOrEmpty(request.Password))
            return BadRequest("Số điện thoại và mật khẩu không được để trống");

        // Chuẩn hóa số điện thoại
        // Ví dụ: "84901234567" → "0901234567"
        var normalizedPhone = NormalizePhoneNumber(request.PhoneNumber);

        var result = await _authService.LoginAsync(
            normalizedPhone,
            request.Password);

        if (!result.Success)
            return Unauthorized(result);

        Response.Cookies.Append("access_token", result.Token!, new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddHours(8)
        });

        return Ok(new
        {
            result.Success,
            result.Message,
            result.Token,
            result.User
        });
    }

    /// <summary>Đăng xuất hệ thống</summary>
    [HttpPost("logout")]
    public IActionResult Logout()
    {
        Response.Cookies.Delete("access_token");
        return Ok(new { message = "Đăng xuất thành công" });
    }

    /// <summary>Lấy thông tin người dùng hiện tại</summary>
    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new UserInfo
        {
            Id = User.GetUserId(),
            FullName = User.GetFullName(),
            Email = User.GetEmail(),
            PhoneNumber = User.GetPhoneNumber(),
            Role = User.GetRole()
        });
    }

    // ── Helper ────────────────────────────────────────────────
    private static string NormalizePhoneNumber(string phone)
    {
        // Xóa khoảng trắng và dấu gạch ngang
        phone = phone.Replace(" ", "").Replace("-", "");

        // Chuyển +84 hoặc 84 về dạng 0
        if (phone.StartsWith("+84"))
            return "0" + phone[3..];

        if (phone.StartsWith("84") && phone.Length == 11)
            return "0" + phone[2..];

        return phone;
    }
}