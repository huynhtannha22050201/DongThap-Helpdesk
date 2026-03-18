using System.Reflection;
using MongoDB.Driver;
using DongThapHelpdesk.Api.Attributes;
using DongThapHelpdesk.Api.Configurations;

namespace DongThapHelpdesk.Api.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext (MongoDbSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            _database = client.GetDatabase(settings.DatabaseName);
        }
        public IMongoCollection<T> GetCollection<T>()
        {
            var attribute = typeof(T)
                .GetCustomAttribute<BsonCollectionAttribute>();

            if (attribute == null)
                throw new Exception(
                    $"[BsonCollection] attribute is missing on {typeof(T).Name}");

            return _database.GetCollection<T>(attribute.CollectionName);
        }
    }
}
