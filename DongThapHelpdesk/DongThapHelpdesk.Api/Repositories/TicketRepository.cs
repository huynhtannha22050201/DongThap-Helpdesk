using MongoDB.Driver;
using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Repositories;

public class TicketRepository
{
    private readonly IMongoCollection<Ticket> _collection;

    public TicketRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<Ticket>();
    }

    public async Task<List<Ticket>> GetAllAsync()
        => await _collection.Find(_ => true).ToListAsync();

    public async Task<Ticket?> GetByIdAsync(string id)
        => await _collection.Find(t => t.Id == id).FirstOrDefaultAsync();

    public async Task<Ticket?> GetByCodeAsync(string code)
        => await _collection.Find(t => t.TicketCode == code).FirstOrDefaultAsync();

    public async Task<List<Ticket>> GetByDepartmentAsync(
        string departmentId)
        => await _collection
            .Find(t => t.AssignedDepartmentId == departmentId)
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync();
    // Chỉ lấy ticket của đơn vị mình
    // Assignee không thấy ticket của đơn vị khác

    public async Task<List<Ticket>> GetByCitizenIdAsync(
        string citizenId)
        => await _collection
            .Find(t => t.ReporterId == citizenId)
            .SortByDescending(t => t.CreatedAt)
            .ToListAsync();
    // Lấy tất cả ticket của một người dân
    // Citizen chỉ thấy ticket của chính mình

    public async Task<long> CountByCitizenInMonthAsync(
        string citizenId, int month, int year)
        => await _collection
            .CountDocumentsAsync(t =>
                t.ReporterId == citizenId
                && t.CreatedAt.Month == month
                && t.CreatedAt.Year == year);
    // Đếm số ticket của citizen trong tháng
    // Dùng để kiểm tra có phải báo cáo đầu tiên trong tháng không
}