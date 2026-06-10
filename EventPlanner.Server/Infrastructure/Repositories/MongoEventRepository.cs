using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using MongoDB.Bson;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Persistence;

namespace EventPlanner.Server.Infrastructure.Repositories;

public class MongoEventRepository : IEventRepository
{
    private readonly MongoDbContext _context;

    public MongoEventRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(string id)
    {
        return await _context.Events.Find(e => e.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Event @event)
    {
        await _context.Events.InsertOneAsync(@event);
    }

    public async Task UpdateAsync(Event @event)
    {
        await _context.Events.ReplaceOneAsync(e => e.Id == @event.Id, @event);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Events.DeleteOneAsync(e => e.Id == id);
    }

    public async Task<List<Event>> ListAsync(string? categoryId = null, string? searchTerm = null)
    {
        var builder = Builders<Event>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrEmpty(categoryId))
        {
            filter &= builder.Eq(e => e.CategoryId, categoryId);
        }

        if (!string.IsNullOrEmpty(searchTerm))
        {
            filter &= builder.Regex(e => e.Title, new BsonRegularExpression(searchTerm, "i")) |
                      builder.Regex(e => e.Description, new BsonRegularExpression(searchTerm, "i"));
        }

        return await _context.Events.Find(filter).ToListAsync();
    }

    public async Task<List<Event>> ListByOrganizerAsync(string organizerId)
    {
        return await _context.Events.Find(e => e.OrganizerId == organizerId).ToListAsync();
    }

    public async Task<List<Event>> ListPublishedAsync()
    {
        return await _context.Events.Find(e => e.Status == EventStatus.Published).ToListAsync();
    }
}
