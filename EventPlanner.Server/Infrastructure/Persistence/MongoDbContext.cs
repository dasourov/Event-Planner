using MongoDB.Driver;
using Microsoft.Extensions.Options;
using EventPlanner.Server.Domain.Entities;

namespace EventPlanner.Server.Infrastructure.Persistence;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IOptions<MongoDbSettings> settings, IMongoClient mongoClient)
    {
        _database = mongoClient.GetDatabase(settings.Value.DatabaseName);
    }

    public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
    public IMongoCollection<Event> Events => _database.GetCollection<Event>("Events");
    public IMongoCollection<Booking> Bookings => _database.GetCollection<Booking>("Bookings");
    public IMongoCollection<Comment> Comments => _database.GetCollection<Comment>("Comments");
    public IMongoCollection<Category> Categories => _database.GetCollection<Category>("Categories");

    public async Task EnsureIndexesAsync()
    {
        // Events: indexes on commonly filtered/sorted fields
        await Events.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<Event>(
                Builders<Event>.IndexKeys.Ascending(e => e.CategoryId)),
            new CreateIndexModel<Event>(
                Builders<Event>.IndexKeys.Ascending(e => e.Status)),
            new CreateIndexModel<Event>(
                Builders<Event>.IndexKeys.Ascending(e => e.OrganizerId)),
            new CreateIndexModel<Event>(
                Builders<Event>.IndexKeys.Ascending(e => e.Date))
        });

        // Bookings: indexes for event and user lookups
        await Bookings.Indexes.CreateManyAsync(new[]
        {
            new CreateIndexModel<Booking>(
                Builders<Booking>.IndexKeys.Ascending(b => b.EventId)),
            new CreateIndexModel<Booking>(
                Builders<Booking>.IndexKeys.Ascending(b => b.UserId))
        });

        // Comments: index on EventId for listing comments by event
        await Comments.Indexes.CreateOneAsync(
            new CreateIndexModel<Comment>(
                Builders<Comment>.IndexKeys.Ascending(c => c.EventId)));
    }
}
