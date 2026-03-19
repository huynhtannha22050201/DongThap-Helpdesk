using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Models;
using DongThapHelpDesk.Api.Models;
using MongoDB.Driver;

namespace DongThapHelpdesk.Api.Repositories;

public class PointHistoryRepository
{
    private readonly IMongoCollection<PointHistory> _collection;

    public PointHistoryRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<PointHistory>();
    }

    public async Task<List<PointHistory>> GetByCitizenIdAsync(
        string citizenId)
        => await _collection
            .Find(p => p.CitizenId == citizenId)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    // Lấy toàn bộ lịch sử điểm của một người dân
    // Sắp xếp mới nhất lên đầu

    public async Task<List<PointHistory>> GetByCitizenIdAsync(
        string citizenId, int month, int year)
        => await _collection
            .Find(p => p.CitizenId == citizenId
                && p.CreatedAt.Month == month
                && p.CreatedAt.Year == year)
            .SortByDescending(p => p.CreatedAt)
            .ToListAsync();
    // Lấy lịch sử điểm theo tháng cụ thể

    public async Task CreateAsync(PointHistory pointHistory)
        => await _collection.InsertOneAsync(pointHistory);
    // Ghi một bản ghi điểm mới
}