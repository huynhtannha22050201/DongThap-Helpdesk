using MongoDB.Driver;
using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Services;

public class TicketCodeGenerator
{
    private readonly MongoDbContext _context;

    public TicketCodeGenerator(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<string> GenerateAsync()
    {
        var now = DateTime.UtcNow;
        var prefix = $"PA-{now:MMyyyy}";

        // Đếm số ticket trong tháng hiện tại
        var collection = _context.GetCollection<Ticket>();
        var count = await collection
            .CountDocumentsAsync(t =>
                t.TicketCode.StartsWith(prefix));

        // Tạo mã theo định dạng PA-032026-001
        var sequence = (count + 1).ToString("D3");
        return $"{prefix}-{sequence}";
    }
}