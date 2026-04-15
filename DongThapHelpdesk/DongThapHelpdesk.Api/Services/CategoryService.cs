using DongThapHelpdesk.Api.DTOs.Category;
using DongThapHelpdesk.Api.Models;
using DongThapHelpdesk.Api.Repositories;

namespace DongThapHelpdesk.Api.Services;

public class CategoryService
{
    private readonly CategoryRepository _repo;

    public CategoryService(CategoryRepository repo)
    {
        _repo = repo;
    }

    // Lấy danh sách phẳng để hiển thị trong form
    public async Task<List<CategoryResponse>> GetFlatAsync()
    {
        var all = await _repo.GetAllAsync();
        return all.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code,
            DefaultSlaHours = c.DefaultSlaHours,
            Description = c.Description,
            IsActive = c.IsActive
        }).ToList();
    }

    public async Task<CategoryResponse?> GetByIdAsync(string id)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category == null) return null;

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Code = category.Code,
            DefaultSlaHours = category.DefaultSlaHours,
            Description = category.Description,
            IsActive = category.IsActive
        };
    }

    public async Task<(bool Success, string Message,
        CategoryResponse? Data)> CreateAsync(
        CreateCategoryRequest request)
    {
        // Kiểm tra code đã tồn tại chưa
        var existing = await _repo.GetByCodeAsync(request.Code);
        if (existing != null)
            return (false, "Mã danh mục đã tồn tại", null);

        var category = new IncidentCategory
        {
            Name = request.Name,
            Code = request.Code.ToUpper(),
            DefaultSlaHours = request.DefaultSlaHours,
            Description = request.Description,
            IsActive = true
        };

        await _repo.CreateAsync(category);

        return (true, "Tạo danh mục thành công",
            new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                DefaultSlaHours = category.DefaultSlaHours,
                Description = category.Description,
                IsActive = category.IsActive
            });
    }

    public async Task<(bool Success, string Message)>
        UpdateAsync(string id, UpdateCategoryRequest request)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category == null)
            return (false, "Không tìm thấy danh mục");

        category.Name = request.Name;
        category.Code = request.Code.ToUpper();
        category.DefaultSlaHours = request.DefaultSlaHours;
        category.Description = request.Description;
        category.IsActive = request.IsActive;

        await _repo.UpdateAsync(category);
        return (true, "Cập nhật danh mục thành công");
    }

    public async Task<(bool Success, string Message)>
        DeleteAsync(string id)
    {
        var category = await _repo.GetByIdAsync(id);
        if (category == null)
            return (false, "Không tìm thấy danh mục");

        await _repo.SoftDeleteAsync(id);
        return (true, "Xóa danh mục thành công");
    }

    public async Task<(List<CategoryResponse> Items, long Total)>
    GetPagedAsync(int page, int pageSize, string? search = null,
    bool? isActive = null, string? sortField = null, string? sortDir = null)
    {
        var items = await _repo.GetPagedAsync(page, pageSize, search, isActive, sortField, sortDir);
        var total = await _repo.CountAsync(search, isActive);

        var responses = items.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code,
            Description = c.Description,
            DefaultSlaHours = c.DefaultSlaHours,
            IsActive = c.IsActive
        }).ToList();

        return (responses, total);
    }

    public async Task<object> GetStatsAsync()
    {
        var all = await _repo.GetAllAsync();
        return new
        {
            total = all.Count,
            active = all.Count(c => c.IsActive),
            inactive = all.Count(c => !c.IsActive),
            avgSla = all.Where(c => c.IsActive).Any()
                ? (int)Math.Round(all.Where(c => c.IsActive).Average(c => c.DefaultSlaHours))
                : 0
        };
    }
}