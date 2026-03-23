using MongoDB.Driver;
using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Repositories;

public class CategoryRepository
{
    private readonly IMongoCollection<IncidentCategory> _collection;

    public CategoryRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<IncidentCategory>();
    }

    public async Task<List<IncidentCategory>> GetAllAsync()
        => await _collection
            .Find(_ => true)
            .SortBy(c => c.Name)
            .ToListAsync();

    public async Task<List<IncidentCategory>> GetActiveAsync()
        => await _collection
            .Find(c => c.IsActive)
            .SortBy(c => c.Name)
            .ToListAsync();
    // Chỉ lấy danh mục đang hoạt động
    // Dùng khi hiển thị form gửi báo cáo cho người dân

    public async Task<List<IncidentCategory>> GetRootCategoriesAsync()
        => await _collection
            .Find(c => c.ParentCategoryId == null && c.IsActive)
            .SortBy(c => c.Name)
            .ToListAsync();
    // Lấy danh mục gốc (không có cha)

    public async Task<List<IncidentCategory>> GetChildrenAsync(
        string parentId)
        => await _collection
            .Find(c => c.ParentCategoryId == parentId
                && c.IsActive)
            .SortBy(c => c.Name)
            .ToListAsync();
    // Lấy danh mục con của một danh mục cha

    public async Task<IncidentCategory?> GetByIdAsync(string id)
        => await _collection
            .Find(c => c.Id == id)
            .FirstOrDefaultAsync();

    public async Task<IncidentCategory?> GetByCodeAsync(
        string code)
        => await _collection
            .Find(c => c.Code == code)
            .FirstOrDefaultAsync();

    public async Task CreateAsync(IncidentCategory category)
        => await _collection.InsertOneAsync(category);

    public async Task UpdateAsync(IncidentCategory category)
        => await _collection.ReplaceOneAsync(
            c => c.Id == category.Id, category);

    public async Task<bool> SoftDeleteAsync(string id)
    {
        // Xóa mềm — chỉ đặt IsActive = false
        var update = Builders<IncidentCategory>.Update
            .Set(c => c.IsActive, false);
        var result = await _collection.UpdateOneAsync(
            c => c.Id == id, update);
        return result.ModifiedCount > 0;
    }
}