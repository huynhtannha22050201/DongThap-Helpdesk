using DongThapHelpdesk.Api.DTOs.Department;
using DongThapHelpdesk.Api.Enums;
using DongThapHelpdesk.Api.Models;
using DongThapHelpdesk.Api.Repositories;

namespace DongThapHelpdesk.Api.Services;

public class DepartmentService
{
    private readonly DepartmentRepository _repo;
    private readonly UserRepository _userRepo;

    public DepartmentService(
        DepartmentRepository repo,
        UserRepository userRepo)
    {
        _repo = repo;
        _userRepo = userRepo;
    }

    public async Task<List<DepartmentResponse>> GetAllAsync()
    {
        var departments = await _repo.GetActiveAsync();
        return await MapToResponseListAsync(departments);
    }

    public async Task<DepartmentResponse?> GetByIdAsync(
        string id)
    {
        var department = await _repo.GetByIdAsync(id);
        if (department == null) return null;
        return await MapToResponseAsync(department);
    }

    public async Task<(bool Success, string Message,
        DepartmentResponse? Data)> CreateAsync(
        CreateDepartmentRequest request)
    {
        // Kiểm tra code đã tồn tại chưa
        var existing = await _repo.GetByCodeAsync(request.Code);
        if (existing != null)
            return (false, "Mã đơn vị đã tồn tại", null);

        if (!Enum.TryParse<DepartmentLevel>(
            request.Level, out var level))
            return (false, "Cấp hành chính không hợp lệ", null);

        var department = new Department
        {
            Name = request.Name,
            Code = request.Code.ToUpper(),
            Level = level,
            ParentId = request.ParentId,
            ResponsibleUserId = request.ResponsibleUserId,
            IsActive = true
        };

        await _repo.CreateAsync(department);
        return (true, "Tạo đơn vị thành công",
            await MapToResponseAsync(department));
    }

    public async Task<(bool Success, string Message)>
        UpdateAsync(string id, UpdateDepartmentRequest request)
    {
        var department = await _repo.GetByIdAsync(id);
        if (department == null)
            return (false, "Không tìm thấy đơn vị");

        department.Name = request.Name;
        department.Code = request.Code.ToUpper();
        department.ResponsibleUserId =
            request.ResponsibleUserId;
        department.IsActive = request.IsActive;

        await _repo.UpdateAsync(department);
        return (true, "Cập nhật đơn vị thành công");
    }

    public async Task<(bool Success, string Message)>
        DeleteAsync(string id)
    {
        var department = await _repo.GetByIdAsync(id);
        if (department == null)
            return (false, "Không tìm thấy đơn vị");

        await _repo.SoftDeleteAsync(id);
        return (true, "Xóa đơn vị thành công");
    }

    // ── Helpers ───────────────────────────────────────────
    private async Task<List<DepartmentResponse>>
        MapToResponseListAsync(List<Department> departments)
    {
        var result = new List<DepartmentResponse>();
        foreach (var d in departments)
            result.Add(await MapToResponseAsync(d));
        return result;
    }

    private async Task<DepartmentResponse> MapToResponseAsync(
        Department d)
    {
        var parent = d.ParentId != null
            ? await _repo.GetByIdAsync(d.ParentId)
            : null;

        var responsible = d.ResponsibleUserId != null
            ? await _userRepo.GetByIdAsync(
                d.ResponsibleUserId)
            : null;

        return new DepartmentResponse
        {
            Id = d.Id,
            Name = d.Name,
            Code = d.Code,
            Level = d.Level.ToString(),
            ParentId = d.ParentId,
            ParentName = parent?.Name,
            ResponsibleUserId = d.ResponsibleUserId,
            ResponsibleUserName = responsible?.FullName,
            IsActive = d.IsActive
        };
    }
}