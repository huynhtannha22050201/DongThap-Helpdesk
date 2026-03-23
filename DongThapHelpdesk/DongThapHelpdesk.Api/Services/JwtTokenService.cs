using DongThapHelpdesk.Api.Configurations;
using DongThapHelpdesk.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DongThapHelpdesk.Api.Services;

public class JwtTokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(JwtSettings jwtSettings)
    {
        _jwtSettings = jwtSettings;
    }

    public string GenerateToken(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            // ID người dùng

            new(ClaimTypes.Name, user.FullName),
            // Họ tên người dùng

            new(ClaimTypes.Email, user.Email),
            // Email người dùng

            new(ClaimTypes.MobilePhone, user.PhoneNumber),
            // Số điện thoại

            new(ClaimTypes.Role, user.Role.ToString()),
            // Vai trò: Dispatcher, Assignee...

            new("DepartmentId", user.DepartmentId ?? ""),
            // ID đơn vị công tác
        };

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));

        var credentials = new SigningCredentials(
            key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(_jwtSettings.ExpiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}