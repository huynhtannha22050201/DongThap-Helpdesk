using MongoDB.Driver;
using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Repositories;

public class RatingRepository
{
    private readonly IMongoCollection<Rating> _collection;

    public RatingRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<Rating>();
    }

    public async Task<Rating?> GetByTicketIdAsync(
        string ticketId)
        => await _collection
            .Find(r => r.TicketId == ticketId)
            .FirstOrDefaultAsync();

    public async Task<Rating?> GetOneByFilterAsync(
        FilterDefinition<Rating> filter)
        => await _collection
            .Find(filter)
            .FirstOrDefaultAsync();

    public async Task<string> CreateAsync(Rating rating)
    {
        await _collection.InsertOneAsync(rating);
        return rating.Id;
    }
}