using MongoDB.Driver;
using DongThapHelpdesk.Api.Data;

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
}