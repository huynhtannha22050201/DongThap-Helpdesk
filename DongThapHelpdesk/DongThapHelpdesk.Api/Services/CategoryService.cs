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

    // Lấy toàn bộ dạng cây phân cấp
    public async Task<List<CategoryResponse>> GetTreeAsync()
    {
        var all = await _repo.GetActiveAsync();
        var roots = all
            .Where(c => c.ParentCategoryId == null)
            .ToList();

        return roots.Select(r => BuildTree(r, all)).ToList();
    }

    // Lấy danh sách phẳng để hiển thị trong form
    public async Task<List<CategoryResponse>> GetFlatAsync()
    {
        var all = await _repo.GetActiveAsync();
        return all.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Code = c.Code,
            ParentCategoryId = c.ParentCategoryId,
            ParentCategoryName = c.ParentCategoryId != null
                ? all.FirstOrDefault(p =>
                    p.Id == c.ParentCategoryId)?.Name
                : null,
            DefaultSlaHours = c.DefaultSlaHours,
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
            ParentCategoryId = category.ParentCategoryId,
            DefaultSlaHours = category.DefaultSlaHours,
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
            ParentCategoryId = request.ParentCategoryId,
            DefaultSlaHours = request.DefaultSlaHours,
            IsActive = true
        };

        await _repo.CreateAsync(category);

        return (true, "Tạo danh mục thành công",
            new CategoryResponse
            {
                Id = category.Id,
                Name = category.Name,
                Code = category.Code,
                ParentCategoryId = category.ParentCategoryId,
                DefaultSlaHours = category.DefaultSlaHours,
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

    // ── Helper: Xây dựng cây phân cấp ────────────────────
    private static CategoryResponse BuildTree(
        IncidentCategory node,
        List<IncidentCategory> all)
    {
        var children = all
            .Where(c => c.ParentCategoryId == node.Id)
            .ToList();

        return new CategoryResponse
        {
            Id = node.Id,
            Name = node.Name,
            Code = node.Code,
            ParentCategoryId = node.ParentCategoryId,
            DefaultSlaHours = node.DefaultSlaHours,
            IsActive = node.IsActive,
            Children = children
                .Select(c => BuildTree(c, all))
                .ToList()
        };
    }
}