using MongoDB.Driver;
using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Repositories;

public class DepartmentRepository
{
    private readonly IMongoCollection<Department> _collection;

    public DepartmentRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<Department>();
    }

    public async Task<List<Department>> GetAllAsync()
        => await _collection
            .Find(_ => true)
            .SortByDescending(d => d.CreatedAt)
            .ToListAsync();

    public async Task<List<Department>> GetActiveAsync()
        => await _collection
            .Find(d => d.IsActive)
            .SortBy(d => d.Name)
            .ToListAsync();

    public async Task<Department?> GetByIdAsync(string id)
        => await _collection
            .Find(d => d.Id == id)
            .FirstOrDefaultAsync();

    public async Task<Department?> GetByCodeAsync(string code)
        => await _collection
            .Find(d => d.Code == code)
            .FirstOrDefaultAsync();

    public async Task CreateAsync(Department department)
        => await _collection.InsertOneAsync(department);

    public async Task UpdateAsync(Department department)
        => await _collection.ReplaceOneAsync(
            d => d.Id == department.Id, department);

    public async Task<bool> SoftDeleteAsync(string id)
    {
        var update = Builders<Department>.Update
            .Set(d => d.IsActive, false);
        var result = await _collection.UpdateOneAsync(
            d => d.Id == id, update);
        return result.ModifiedCount > 0;
    }

    public async Task<List<Department>> GetPagedAsync(
    int page, int pageSize, string? search = null, bool? isActive = null)
    {
        var filter = Builders<Department>.Filter.Empty;

        if (!string.IsNullOrEmpty(search))
        {
            filter = Builders<Department>.Filter.Or(
                Builders<Department>.Filter.Regex(c => c.Name, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                Builders<Department>.Filter.Regex(c => c.Code, new MongoDB.Bson.BsonRegularExpression(search, "i"))
            );
        }

        // Lọc trạng thái nếu có
        if (isActive.HasValue)
        {
            var activeFilter = Builders<Department>.Filter.Eq(c => c.IsActive, isActive.Value);
            filter = filter == Builders<Department>.Filter.Empty
                ? activeFilter
                : Builders<Department>.Filter.And(filter, activeFilter);
        }

        return await _collection
            .Find(filter)
            .SortByDescending(c => c.Id)
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync();
    }

    public async Task<long> CountAsync(string? search = null, bool? isActive = null)
    {
        var filter = Builders<Department>.Filter.Empty;

        if (!string.IsNullOrEmpty(search))
        {
            filter = Builders<Department>.Filter.Or(
                Builders<Department>.Filter.Regex(c => c.Name, new MongoDB.Bson.BsonRegularExpression(search, "i")),
                Builders<Department>.Filter.Regex(c => c.Code, new MongoDB.Bson.BsonRegularExpression(search, "i"))
            );
        }

        if (isActive.HasValue)
        {
            var activeFilter = Builders<Department>.Filter.Eq(c => c.IsActive, isActive.Value);
            filter = filter == Builders<Department>.Filter.Empty
                ? activeFilter
                : Builders<Department>.Filter.And(filter, activeFilter);
        }

        return await _collection.CountDocumentsAsync(filter);
    }
}