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
}
