using DongThapHelpdesk.Api.Constants;
using DongThapHelpdesk.Api.DTOs.Category;
using DongThapHelpdesk.Api.DTOs.Department;
using DongThapHelpdesk.Api.Enums;
using DongThapHelpdesk.Api.Models;
using DongThapHelpdesk.Api.Repositories;
using MongoDB.Driver;

namespace DongThapHelpdesk.Api.Services;

public class DepartmentService
{
    private readonly DepartmentRepository _repo;
    private readonly UserRepository _userRepo;
    private readonly TicketRepository _ticketRepo;

    public DepartmentService(
        DepartmentRepository repo,
        UserRepository userRepo,
        TicketRepository ticketRepo)
    {
        _repo = repo;
        _userRepo = userRepo;
        _ticketRepo = ticketRepo;
    }

    public async Task<List<DepartmentResponse>> GetAllAsync()
    {
        var departments = await _repo.GetAllAsync(); // Lấy từ MongoDB
        var responseList = new List<DepartmentResponse>();

        foreach (var dept in departments)
        {
            var dto = await MapToResponseAsync(dept); // Hàm convert sang DTO

            if (!string.IsNullOrEmpty(dept.ResponsibleUserId))
            {
                // Tìm thông tin User từ UserRepository
                var user = await _userRepo.GetByIdAsync(dept.ResponsibleUserId);
                dto.ResponsibleUserName = user?.FullName; // Gán tên vào DTO
            }
            dto.StaffCount = await _userRepo.CountAssigneesByDepartmentAsync(dept.Id);

            dto.TicketCount = await _ticketRepo.CountByDepartmentAsync(dept.Id);

            responseList.Add(dto);
        }
        return responseList;
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

        if (level == DepartmentLevel.Province)
        {
            // Chỉ cho phép tồn tại 1 đơn vị cấp tỉnh duy nhất
            var existingProvince = (await _repo.GetActiveAsync())
                .FirstOrDefault(d => d.Level == DepartmentLevel.Province);
            if (existingProvince != null)
                return (false, "Đã tồn tại đơn vị cấp tỉnh", null);

            // Cấp tỉnh không có đơn vị cha
            request.ParentId = null;
        }
        else
        {
            // Cấp xã/phường bắt buộc phải thuộc đơn vị cấp tỉnh
            if (string.IsNullOrEmpty(request.ParentId))
                return (false, "Đơn vị cấp xã/phường phải có đơn vị cấp trên", null);

            var parent = await _repo.GetByIdAsync(request.ParentId);
            if (parent == null || parent.Level != DepartmentLevel.Province)
                return (false, "Đơn vị cấp trên phải là cấp tỉnh", null);
        }

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
            ResponsibleUserPhone = responsible?.PhoneNumber,
            ResponsibleUserEmail = responsible?.Email,
            IsActive = d.IsActive
        };
    }

    public async Task<object> GetPagedAsync(
    int page, int pageSize, string? search, bool? isActive,
    string? sortBy = null, string? sortDir = "desc")
    {
        var all = (await _repo.GetAllAsync())
            .Where(d => d.Level != DepartmentLevel.Province)
            .ToList();

        // Lấy tất cả users (staff) và tickets để đếm theo department
        var allUsers = await _userRepo.GetByFilterAsync(
            Builders<AppUser>.Filter.Ne(u => u.Role, UserRole.Citizen));
        var allTickets = await _ticketRepo.GetAllAsync();

        // Đếm sẵn theo departmentId
        var staffCountMap = allUsers
            .Where(u => u.DepartmentId != null)
            .GroupBy(u => u.DepartmentId!)
            .ToDictionary(g => g.Key, g => g.Count());

        var ticketCountMap = allTickets
            .Where(t => t.AssignedDepartmentId != null)
            .GroupBy(t => t.AssignedDepartmentId!)
            .ToDictionary(g => g.Key, g => g.Count());

        // Stats
        var stats = new
        {
            total = all.Count,
            active = all.Count(d => d.IsActive),
            inactive = all.Count(d => !d.IsActive),
            hasManager = all.Count(d => d.ResponsibleUserId != null),
        };

        // Filter
        var filtered = all.AsEnumerable();
        if (isActive.HasValue)
            filtered = filtered.Where(d => d.IsActive == isActive.Value);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var q = search.ToLower();
            filtered = filtered.Where(d =>
                d.Name.ToLower().Contains(q) ||
                d.Code.ToLower().Contains(q));
        }

        var sorted = filtered.AsEnumerable();
        var isAsc = sortDir?.ToLower() == "asc";

        sorted = sortBy?.ToLower() switch
        {
            "name" => isAsc
                ? sorted.OrderBy(d => d.Name)
                : sorted.OrderByDescending(d => d.Name),
            "isactive" => isAsc
                ? sorted.OrderBy(d => d.IsActive)
                : sorted.OrderByDescending(d => d.IsActive),
            _ => sorted.OrderByDescending(d => d.CreatedAt), // Mặc định: mới nhất trước
        };

        var list = sorted.ToList();
        var total = list.Count;
        var totalPages = (int)Math.Ceiling(total / (double)pageSize);

        var paged = list
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // Map sang response kèm staffCount + ticketCount
        var items = new List<object>();
        foreach (var d in paged)
        {
            var resp = await MapToResponseAsync(d);
            items.Add(new
            {
                resp.Id,
                resp.Name,
                resp.Code,
                resp.Level,
                resp.ParentId,
                resp.ParentName,
                resp.ResponsibleUserId,
                resp.ResponsibleUserName,
                resp.ResponsibleUserPhone,
                resp.ResponsibleUserEmail,
                resp.IsActive,
                // Đếm user thuộc department + chủ tịch (nếu có)
                staffCount = staffCountMap.GetValueOrDefault(d.Id, 0)
                    + (d.ResponsibleUserId != null
                        && !staffCountMap.ContainsKey(d.Id) ? 0
                        // Nếu chủ tịch đã nằm trong staffCountMap thì không cộng thêm
                        : 0),
                ticketCount = ticketCountMap.GetValueOrDefault(d.Id, 0),
            });
        }

        return new { items, total, totalPages, stats };
    }



    public async Task<object> GetStatsAsync()
    {
        // 1. Lấy tất cả danh sách phòng ban
        var allDepartments = await _repo.GetAllAsync();

        // 2. Lấy danh sách cán bộ (Staff) từ UserRepository để đếm tổng số cán bộ
        // (Thay đổi tên hàm GetUsersByRoleAsync tùy thuộc vào cách bạn viết trong _userRepo)
        var allStaffs = await _userRepo.GetUsersByRole(Roles.Assignee);

        // Tính toán các chỉ số
        var total = allDepartments.Count;

        var active = allDepartments.Count(d => d.IsActive);

        // Tổng số lượng trưởng phòng: Lấy các ResponsibleUserId có dữ liệu, loại bỏ trùng lặp (Distinct) và đếm
        var totalManagers = allDepartments
            .Where(d => !string.IsNullOrEmpty(d.ResponsibleUserId))
            .Select(d => d.ResponsibleUserId)
            .Distinct()
            .Count();

        var totalStaff = allStaffs.Count;

        // Trả về một object ẩn danh (anonymous object) khớp với dữ liệu frontend cần
        return new
        {
            total = total,
            active = active,
            totalStaff = totalStaff,
            totalManagers = totalManagers
        };
    }
}