using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MongoDB.Driver;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Infrastructure.Persistence;

namespace EventPlanner.Server.Infrastructure.Repositories;

public class MongoBookingRepository : IBookingRepository
{
    private readonly MongoDbContext _context;

    public MongoBookingRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(string id)
    {
        return await _context.Bookings.Find(b => b.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Booking?> GetByUserAndEventAsync(string userId, string eventId)
    {
        return await _context.Bookings.Find(b => b.UserId == userId && b.EventId == eventId).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Booking booking)
    {
        await _context.Bookings.InsertOneAsync(booking);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Bookings.DeleteOneAsync(b => b.Id == id);
    }

    public async Task<List<Booking>> ListByUserAsync(string userId)
    {
        return await _context.Bookings.Find(b => b.UserId == userId).ToListAsync();
    }

    public async Task<List<Booking>> ListByEventAsync(string eventId)
    {
        return await _context.Bookings.Find(b => b.EventId == eventId).ToListAsync();
    }

    public async Task<int> CountByEventAsync(string eventId)
    {
        var count = await _context.Bookings.CountDocumentsAsync(b => b.EventId == eventId);
        return (int)count;
    }

    public async Task<Dictionary<string, int>> GetAttendeeCountsAsync(IEnumerable<string> eventIds)
    {
        var filter = Builders<Booking>.Filter.In(b => b.EventId, eventIds);
        var counts = await _context.Bookings.Aggregate()
            .Match(filter)
            .Group(b => b.EventId, g => new { EventId = g.Key, Count = g.Count() })
            .ToListAsync();
        return counts.ToDictionary(c => c.EventId, c => c.Count);
    }
}
