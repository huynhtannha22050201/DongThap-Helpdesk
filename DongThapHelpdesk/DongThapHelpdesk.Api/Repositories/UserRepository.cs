using MongoDB.Driver;
using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Repositories;

public class UserRepository
{
    private readonly IMongoCollection<AppUser> _collection;

    public UserRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<AppUser>();
    }

    public async Task<List<AppUser>> GetUsersByRole(string role) 
        => await _collection
            .Find(u => u.Role.ToString() == role)
            .ToListAsync();

    public async Task<AppUser?> GetByPhoneNumberAsync(string phoneNumber)
        => await _collection
            .Find(u => u.PhoneNumber == phoneNumber)
            .FirstOrDefaultAsync();

    public async Task<AppUser?> GetByIdAsync(string id)
        => await _collection
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync();

    public async Task<List<AppUser>> GetAllAsync()
        => await _collection
            .Find(_ => true)
            .SortBy(u => u.FullName)
            .ToListAsync();

    public async Task<List<AppUser>> GetTopByFilterAsync(
        FilterDefinition<AppUser> filter,
        SortDefinition<AppUser> sort,
        int limit)
        => await _collection
            .Find(filter)
            .Sort(sort)
            .Limit(limit)
            .ToListAsync();

    public async Task<List<AppUser>> GetByFilterAsync(
        FilterDefinition<AppUser> filter)
        => await _collection
            .Find(filter)
            .ToListAsync();

    public async Task CreateAsync(AppUser user)
        => await _collection.InsertOneAsync(user);

    public async Task UpdateAsync(AppUser user)
        => await _collection.ReplaceOneAsync(
            u => u.Id == user.Id, user);
    // Cập nhật toàn bộ thông tin user
    // Dùng khi cập nhật điểm sau mỗi hành động

    public async Task UpdateFieldsAsync(
        string id,
        UpdateDefinition<AppUser> update)
        => await _collection.UpdateOneAsync(
            u => u.Id == id, update);

    public async Task UpdateManyFieldsAsync(
        FilterDefinition<AppUser> filter,
        UpdateDefinition<AppUser> update)
        => await _collection.UpdateManyAsync(
            filter, update);

    public async Task<List<AppUser>> GetMonthlyLeaderboardAsync(
        int top = 10)
        => await _collection
            .Find(u => u.Role == Enums.UserRole.Citizen
                && u.CitizenProfile != null)
            .SortByDescending(u =>
                u.CitizenProfile!.CurrentMonthPoints)
            .Limit(top)
            .ToListAsync();
    // Lấy top người dân có điểm cao nhất trong tháng

    public async Task<List<AppUser>> GetQuarterlyLeaderboardAsync(
        int top = 10)
        => await _collection
            .Find(u => u.Role == Enums.UserRole.Citizen
                && u.CitizenProfile != null)
            .SortByDescending(u =>
                u.CitizenProfile!.CurrentQuarterPoints)
            .Limit(top)
            .ToListAsync();
    // Lấy top người dân có điểm cao nhất trong quý

    public async Task ResetMonthlyPointsAsync()
    {
        var update = Builders<AppUser>.Update
            .Set(u => u.CitizenProfile!.CurrentMonthPoints, 0);
        await _collection.UpdateManyAsync(
            u => u.Role == Enums.UserRole.Citizen
                && u.CitizenProfile != null,
            update);
    }
    // Reset điểm tháng về 0 vào đầu tháng mới
    // Gọi bởi Background Service

    public async Task ResetQuarterlyPointsAsync()
    {
        var update = Builders<AppUser>.Update
            .Set(u => u.CitizenProfile!.CurrentQuarterPoints, 0);
        await _collection.UpdateManyAsync(
            u => u.Role == Enums.UserRole.Citizen
                && u.CitizenProfile != null,
            update);
    }
    // Reset điểm quý về 0 vào đầu quý mới
    // Gọi bởi Background Service

    public async Task<long> CountTotalAssigneesAsync()
        => await _collection.CountDocumentsAsync(
            Builders<AppUser>.Filter.Eq(u => u.Role, Enums.UserRole.Assignee));

    public async Task<long> CountAssigneesByDepartmentAsync(string departmentId)
        => await _collection.CountDocumentsAsync(
            Builders<AppUser>.Filter.And(
                Builders<AppUser>.Filter.Eq(u => u.DepartmentId, departmentId),
                Builders<AppUser>.Filter.Eq(u => u.Role, Enums.UserRole.Assignee)
            ));
}