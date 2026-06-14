using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
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

    public async Task<List<Event>> ListAsync(
        string? categoryId = null,
        string? searchTerm = null,
        string? status = null,
        int page = 1,
        int pageSize = 20
    )
    {
        var builder = Builders<Event>.Filter;
        var filter = builder.Empty;

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            filter &= builder.Eq(e => e.CategoryId, categoryId);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var safeSearchTerm = Regex.Escape(searchTerm.Trim());

            var searchFilter =
                builder.Regex(e => e.Title, new BsonRegularExpression(safeSearchTerm, "i")) |
                builder.Regex(e => e.Description, new BsonRegularExpression(safeSearchTerm, "i")) |
                builder.Regex(e => e.Location, new BsonRegularExpression(safeSearchTerm, "i"));

            filter &= searchFilter;
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<EventStatus>(status, true, out var statusValue))
        {
            filter &= builder.Eq(e => e.Status, statusValue);
        }

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var skip = (page - 1) * pageSize;

        return await _context.Events
            .Find(filter)
            .SortBy(e => e.Date)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync();
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