using System.Security.Claims;

namespace DongThapHelpdesk.Api.Extensions;

public static class ClaimExtensions
{
    public static string GetUserId(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.NameIdentifier)?.Value!;
    // Lấy ID người dùng từ Token
    // Dùng khi cần biết ai đang thực hiện hành động

    public static string GetFullName(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Name)?.Value!;
    // Lấy họ tên người dùng

    public static string GetEmail(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Email)?.Value!;
    // Lấy email người dùng

    public static string GetPhoneNumber(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.MobilePhone)?.Value!;
    // Lấy số điện thoại người dùng

    public static string GetRole(this ClaimsPrincipal user)
        => user.FindFirst(ClaimTypes.Role)?.Value!;
    // Lấy vai trò người dùng
    // Ví dụ: "Dispatcher", "Assignee", "Manager"

    public static string GetDepartmentId(this ClaimsPrincipal user)
        => user.FindFirst("DepartmentId")?.Value!;
    // Lấy ID đơn vị của cán bộ
    // Null nếu là Admin hoặc Manager cấp tỉnh

    public static bool IsInRole(this ClaimsPrincipal user, string role)
        => user.GetRole() == role;
    // Kiểm tra người dùng có đúng vai trò không
    // Ví dụ: User.IsInRole("Dispatcher")
}