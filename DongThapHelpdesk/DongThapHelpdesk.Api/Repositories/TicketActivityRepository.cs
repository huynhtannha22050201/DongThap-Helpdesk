using MongoDB.Driver;
using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Repositories;

public class TicketActivityRepository
{
    private readonly IMongoCollection<TicketActivity>
        _collection;

    public TicketActivityRepository(MongoDbContext context)
    {
        _collection = context
            .GetCollection<TicketActivity>();
    }

    public async Task<List<TicketActivity>>
        GetByFilterAsync(
        FilterDefinition<TicketActivity> filter)
        => await _collection
            .Find(filter)
            .SortByDescending(a => a.CreatedAt)
            .ToListAsync();

    public async Task<string> CreateAsync(
        TicketActivity activity)
    {
        await _collection.InsertOneAsync(activity);
        return activity.Id;
    }
}