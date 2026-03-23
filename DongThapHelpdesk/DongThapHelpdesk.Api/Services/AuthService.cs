using DongThapHelpdesk.Api.DTOs.Auth;
using DongThapHelpdesk.Api.Repositories;

namespace DongThapHelpdesk.Api.Services;

public class AuthService
{
    private readonly UserRepository _userRepo;
    private readonly JwtTokenService _jwtTokenService;

    public AuthService(
        UserRepository userRepo,
        JwtTokenService jwtTokenService)
    {
        _userRepo = userRepo;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<LoginResult> LoginAsync(
        string phoneNumber, string password)
    {
        // Tìm user theo số điện thoại
        var user = await _userRepo.GetByPhoneNumberAsync(phoneNumber);

        if (user == null)
            return new LoginResult
            {
                Success = false,
                Message = "Số điện thoại không tồn tại trong hệ thống"
            };

        if (!user.IsActive)
            return new LoginResult
            {
                Success = false,
                Message = "Tài khoản đã bị khóa, vui lòng liên hệ quản trị viên"
            };

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            return new LoginResult
            {
                Success = false,
                Message = "Mật khẩu không chính xác"
            };

        var token = _jwtTokenService.GenerateToken(user);

        return new LoginResult
        {
            Success = true,
            Message = "Đăng nhập thành công",
            Token = token,
            User = new UserInfo
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role.ToString(),
                DepartmentId = user.DepartmentId
            }
        };
    }
}