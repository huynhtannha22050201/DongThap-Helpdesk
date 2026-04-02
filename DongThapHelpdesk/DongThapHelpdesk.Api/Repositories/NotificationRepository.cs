using MongoDB.Driver;
using DongThapHelpdesk.Api.Data;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Repositories;

public class NotificationRepository
{
    private readonly IMongoCollection<Notification>
        _collection;

    public NotificationRepository(MongoDbContext context)
    {
        _collection = context
            .GetCollection<Notification>();
    }

    public async Task<List<Notification>> GetByFilterAsync(
        FilterDefinition<Notification> filter)
        => await _collection
            .Find(filter)
            .SortByDescending(n => n.CreatedAt)
            .ToListAsync();

    public async Task<long> CountByFilterAsync(
        FilterDefinition<Notification> filter)
        => await _collection.CountDocumentsAsync(filter);

    public async Task CreateAsync(Notification notification)
        => await _collection.InsertOneAsync(notification);

    public async Task UpdateFieldsAsync(
        string id,
        UpdateDefinition<Notification> update)
        => await _collection.UpdateOneAsync(
            n => n.Id == id, update);

    public async Task UpdateManyFieldsAsync(
        FilterDefinition<Notification> filter,
        UpdateDefinition<Notification> update)
        => await _collection.UpdateManyAsync(
            filter, update);
}