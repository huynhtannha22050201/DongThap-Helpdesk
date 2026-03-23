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
            .SortBy(d => d.Name)
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
}