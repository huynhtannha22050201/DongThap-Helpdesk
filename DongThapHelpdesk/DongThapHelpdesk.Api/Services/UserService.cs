using MongoDB.Driver;
using DongThapHelpdesk.Api.DTOs.User;
using DongThapHelpdesk.Api.Enums;
using DongThapHelpdesk.Api.Models;
using DongThapHelpdesk.Api.Repositories;

namespace DongThapHelpdesk.Api.Services;

public class UserService
{
    private readonly UserRepository _userRepo;
    private readonly DepartmentRepository _departmentRepo;

    public UserService(
        UserRepository userRepo,
        DepartmentRepository departmentRepo)
    {
        _userRepo = userRepo;
        _departmentRepo = departmentRepo;
    }

    // ── Lấy danh sách Staff (không bao gồm Citizen) ───────
    public async Task<List<UserResponse>> GetAllStaffAsync()
    {
        var filter = Builders<AppUser>.Filter
            .Ne(u => u.Role, UserRole.Citizen);
        var users = await _userRepo.GetByFilterAsync(filter);
        return await MapToResponseListAsync(users);
    }

    // ── Lấy danh sách theo Role ───────────────────────────
    public async Task<List<UserResponse>> GetByRoleAsync(
        string role)
    {
        if (!Enum.TryParse<UserRole>(role, out var userRole))
            return new List<UserResponse>();

        var filter = Builders<AppUser>.Filter
            .Eq(u => u.Role, userRole);
        var users = await _userRepo.GetByFilterAsync(filter);
        return await MapToResponseListAsync(users);
    }

    // ── Lấy chi tiết một user ─────────────────────────────
    public async Task<UserResponse?> GetByIdAsync(string id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null) return null;
        return await MapToResponseAsync(user);
    }

    // ── Tạo tài khoản cán bộ ─────────────────────────────
    public async Task<(bool Success,
        string Message,
        UserResponse? Data)> CreateAsync(
        CreateUserRequest request)
    {
        // Nghiệp vụ: kiểm tra SĐT đã tồn tại chưa
        var existing = await _userRepo
            .GetByPhoneNumberAsync(request.PhoneNumber);
        if (existing != null)
            return (false,
                "Số điện thoại đã được sử dụng", null);

        // Nghiệp vụ: validate Role
        if (!Enum.TryParse<UserRole>(
            request.Role, out var role))
            return (false, "Vai trò không hợp lệ", null);

        // Nghiệp vụ: Assignee bắt buộc có DepartmentId
        if (role == UserRole.Assignee &&
            string.IsNullOrEmpty(request.DepartmentId))
            return (false,
                "Cán bộ xử lý phải thuộc một đơn vị", null);

        // Nghiệp vụ: kiểm tra Department tồn tại
        if (!string.IsNullOrEmpty(request.DepartmentId))
        {
            var dept = await _departmentRepo
                .GetByIdAsync(request.DepartmentId);
            if (dept == null)
                return (false,
                    "Đơn vị không tồn tại", null);
        }

        var newUser = new AppUser
        {
            FullName = request.FullName,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt
                .HashPassword(request.Password),
            Role = role,
            DepartmentId = request.DepartmentId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CitizenProfile = null
        };

        await _userRepo.CreateAsync(newUser);

        return (true, "Tạo tài khoản thành công",
            await MapToResponseAsync(newUser));
    }

    // ── Cập nhật thông tin cán bộ ─────────────────────────
    public async Task<(bool Success, string Message)>
        UpdateAsync(string id, UpdateUserRequest request)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null)
            return (false, "Không tìm thấy tài khoản");

        // Nghiệp vụ: không cho sửa tài khoản Citizen
        // từ endpoint này
        if (user.Role == UserRole.Citizen)
            return (false,
                "Không thể sửa tài khoản Citizen " +
                "từ endpoint này");

        user.FullName = request.FullName;
        user.Email = request.Email;
        user.DepartmentId = request.DepartmentId;
        user.IsActive = request.IsActive;

        await _userRepo.UpdateAsync(user);
        return (true, "Cập nhật thành công");
    }

    // ── Khóa/Mở khóa tài khoản ───────────────────────────
    public async Task<(bool Success, string Message)>
        ToggleLockAsync(string id)
    {
        var user = await _userRepo.GetByIdAsync(id);
        if (user == null)
            return (false, "Không tìm thấy tài khoản");

        // Nghiệp vụ: không khóa Admin
        if (user.Role == UserRole.Admin)
            return (false,
                "Không thể khóa tài khoản Admin");

        // Nghiệp vụ: toggle trạng thái
        var update = Builders<AppUser>.Update
            .Set(u => u.IsActive, !user.IsActive);
        await _userRepo.UpdateFieldsAsync(id, update);

        var status = user.IsActive ? "khóa" : "mở khóa";
        return (true, $"Đã {status} tài khoản thành công");
    }

    // ── UC19: Đổi mật khẩu ───────────────────────────────
    public async Task<(bool Success, string Message)>
        ChangePasswordAsync(
        string userId,
        ChangePasswordRequest request)
    {
        var user = await _userRepo.GetByIdAsync(userId);
        if (user == null)
            return (false, "Không tìm thấy tài khoản");

        // Nghiệp vụ: kiểm tra mật khẩu hiện tại
        if (!BCrypt.Net.BCrypt.Verify(
            request.CurrentPassword, user.PasswordHash))
            return (false, "Mật khẩu hiện tại không đúng");

        // Nghiệp vụ: kiểm tra mật khẩu mới khớp
        if (request.NewPassword != request.ConfirmPassword)
            return (false,
                "Mật khẩu xác nhận không khớp");

        // Nghiệp vụ: không dùng mật khẩu cũ
        if (BCrypt.Net.BCrypt.Verify(
            request.NewPassword, user.PasswordHash))
            return (false,
                "Mật khẩu mới không được trùng " +
                "mật khẩu cũ");

        var update = Builders<AppUser>.Update
            .Set(u => u.PasswordHash,
                BCrypt.Net.BCrypt
                    .HashPassword(request.NewPassword));

        await _userRepo.UpdateFieldsAsync(userId, update);
        return (true, "Đổi mật khẩu thành công");
    }

    // ── Private helpers ───────────────────────────────────
    private async Task<List<UserResponse>>
        MapToResponseListAsync(List<AppUser> users)
    {
        var result = new List<UserResponse>();
        foreach (var u in users)
            result.Add(await MapToResponseAsync(u));
        return result;
    }

    private async Task<UserResponse> MapToResponseAsync(
        AppUser u)
    {
        var department = u.DepartmentId != null
            ? await _departmentRepo
                .GetByIdAsync(u.DepartmentId)
            : null;

        return new UserResponse
        {
            Id = u.Id,
            FullName = u.FullName,
            Email = u.Email,
            PhoneNumber = u.PhoneNumber,
            Role = u.Role.ToString(),
            DepartmentId = u.DepartmentId,
            DepartmentName = department?.Name,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt
        };
    }
}