using DongThapHelpdesk.Api.DTOs.Auth;
using DongThapHelpdesk.Api.Services;
using DongThapHelpdesk.Api.Extensions;
using Microsoft.AspNetCore.Mvc;
using DongThapHelpdesk.Api.Repositories;

namespace DongThapHelpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;
    private readonly UserRepository _userRepo;

    public AuthController(AuthService authService, UserRepository userRepo)
    {
        _authService = authService;
        _userRepo = userRepo;
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

        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = false,
            SameSite = SameSiteMode.Strict,
        };

        if (request.RememberMe)
        {
            // Ghi nhớ → cookie sống 7 ngày
            cookieOptions.Expires = DateTimeOffset.UtcNow.AddDays(7);
        }

        Response.Cookies.Append("access_token", result.Token!, cookieOptions);

        return Ok(new
        {
            result.Success,
            result.Message,
            //result.Token,
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
    public async Task<IActionResult> Me()
    {
        var userId = User.GetUserId();
        if (string.IsNullOrEmpty(userId))
            return Ok(new { authenticated = false });

        // Query DB để lấy thêm CitizenProfile (address, ward)
        var user = await _userRepo.GetByIdAsync(userId);

        return Ok(new
        {
            authenticated = true,
            user = new UserInfo
            {
                Id = userId,
                FullName = User.GetFullName(),
                Email = User.GetEmail(),
                PhoneNumber = User.GetPhoneNumber(),
                Role = User.GetRole(),
                DepartmentId = User.GetDepartmentId(),
                Address = user?.CitizenProfile?.Address,
                Ward = user?.CitizenProfile?.Ward,
            }
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