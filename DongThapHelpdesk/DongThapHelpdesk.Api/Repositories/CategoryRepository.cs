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
            .SortByDescending(c => c.Id)
            .ToListAsync();

    public async Task<List<IncidentCategory>> GetActiveAsync()
        => await _collection
            .Find(c => c.IsActive)
            .SortBy(c => c.Name)
            .ToListAsync();
    // Chỉ lấy danh mục đang hoạt động
    // Dùng khi hiển thị form gửi báo cáo cho người dân

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

    public async Task<List<IncidentCategory>> GetPagedAsync(
    int page, int pageSize, string? search = null,
    bool? isActive = null, string? sortField = null, string? sortDir = null)
    {
        var filter = Builders<IncidentCategory>.Filter.Empty;

        if (!string.IsNullOrEmpty(search))
        {
            filter = Builders<IncidentCategory>.Filter.Or(
                Builders<IncidentCategory>.Filter.Regex(c => c.Name, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                Builders<IncidentCategory>.Filter.Regex(c => c.Code, new MongoDB.Bson.BsonRegularExpression(search, "i"))
            );
        }

        if (isActive.HasValue)
        {
            var activeFilter = Builders<IncidentCategory>.Filter.Eq(c => c.IsActive, isActive.Value);
            filter = filter == Builders<IncidentCategory>.Filter.Empty
                ? activeFilter
                : Builders<IncidentCategory>.Filter.And(filter, activeFilter);
        }

        // Sort theo field được chỉ định, mặc định mới nhất lên trên
        var sort = BuildSort(sortField, sortDir);

        return await _collection
            .Find(filter)
            .Sort(sort)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    // THÊM helper build sort
    private static SortDefinition<IncidentCategory> BuildSort(
        string? sortField, string? sortDir)
    {
        var builder = Builders<IncidentCategory>.Sort;
        var isDesc = sortDir?.ToLower() == "desc";

        return sortField?.ToLower() switch
        {
            "code" => isDesc ? builder.Descending(c => c.Code) : builder.Ascending(c => c.Code),
            "name" => isDesc ? builder.Descending(c => c.Name) : builder.Ascending(c => c.Name),
            "defaultslahours" => isDesc ? builder.Descending(c => c.DefaultSlaHours) : builder.Ascending(c => c.DefaultSlaHours),
            "isactive" => isDesc ? builder.Descending(c => c.IsActive) : builder.Ascending(c => c.IsActive),
            _ => builder.Descending(c => c.Id)  // Mặc định: mới nhất lên trên
        };
    }

    public async Task<long> CountAsync(string? search = null, bool? isActive = null)
    {
        var filter = Builders<IncidentCategory>.Filter.Empty;

        if (!string.IsNullOrEmpty(search))
        {
            filter = Builders<IncidentCategory>.Filter.Or(
                Builders<IncidentCategory>.Filter.Regex(c => c.Name, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                Builders<IncidentCategory>.Filter.Regex(c => c.Code, new MongoDB.Bson.BsonRegularExpression(search, "i"))
            );
        }

        if (isActive.HasValue)
        {
            var activeFilter = Builders<IncidentCategory>.Filter.Eq(c => c.IsActive, isActive.Value);
            filter = filter == Builders<IncidentCategory>.Filter.Empty
                ? activeFilter
                : Builders<IncidentCategory>.Filter.And(filter, activeFilter);
        }

        return await _collection.CountDocumentsAsync(filter);
    }   
}