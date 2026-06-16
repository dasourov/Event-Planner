using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        return await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<List<Event>> GetByIdsAsync(IEnumerable<string> ids)
    {
        return await _context.Events.Where(e => ids.Contains(e.Id)).ToListAsync();
    }

    public async Task CreateAsync(Event @event)
    {
        await _context.Events.AddAsync(@event);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Event @event)
    {
        _context.Events.Update(@event);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var @event = await _context.Events.FirstOrDefaultAsync(e => e.Id == id);
        if (@event != null)
        {
            _context.Events.Remove(@event);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Event>> ListAsync(
        string? categoryId = null,
        string? searchTerm = null,
        string? status = null,
        int page = 1,
        int pageSize = 20
    )
    {
        var query = _context.Events.AsQueryable();

        if (!string.IsNullOrWhiteSpace(categoryId))
        {
            query = query.Where(e => e.CategoryId == categoryId);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim().ToLower();
            query = query.Where(e => e.Title.ToLower().Contains(term) ||
                                     e.Description.ToLower().Contains(term) ||
                                     e.Location.ToLower().Contains(term));
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            Enum.TryParse<EventStatus>(status, true, out var statusValue))
        {
            query = query.Where(e => e.Status == statusValue);
        }

        page = page < 1 ? 1 : page;
        pageSize = pageSize < 1 ? 20 : pageSize;
        pageSize = pageSize > 100 ? 100 : pageSize;

        var skip = (page - 1) * pageSize;

        return await query
            .OrderBy(e => e.Date)
            .Skip(skip)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<Event>> ListByOrganizerAsync(string organizerId)
    {
        return await _context.Events.Where(e => e.OrganizerId == organizerId).ToListAsync();
    }

    public async Task<List<Event>> ListPublishedAsync()
    {
        return await _context.Events.Where(e => e.Status == EventStatus.Published).ToListAsync();
    }
}