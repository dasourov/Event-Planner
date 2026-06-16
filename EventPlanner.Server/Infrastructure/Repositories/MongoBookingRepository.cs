using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        return await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
    }

    public async Task<Booking?> GetByUserAndEventAsync(string userId, string eventId)
    {
        return await _context.Bookings.FirstOrDefaultAsync(b => b.UserId == userId && b.EventId == eventId);
    }

    public async Task CreateAsync(Booking booking)
    {
        await _context.Bookings.AddAsync(booking);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);
        if (booking != null)
        {
            _context.Bookings.Remove(booking);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Booking>> ListByUserAsync(string userId)
    {
        return await _context.Bookings.Where(b => b.UserId == userId).ToListAsync();
    }

    public async Task<List<Booking>> ListByEventAsync(string eventId)
    {
        return await _context.Bookings.Where(b => b.EventId == eventId).ToListAsync();
    }

    public async Task<int> CountByEventAsync(string eventId)
    {
        return await _context.Bookings.CountAsync(b => b.EventId == eventId);
    }

    public async Task<Dictionary<string, int>> GetAttendeeCountsAsync(IEnumerable<string> eventIds)
    {
        var counts = await _context.Bookings
            .Where(b => eventIds.Contains(b.EventId))
            .GroupBy(b => b.EventId)
            .Select(g => new { EventId = g.Key, Count = g.Count() })
            .ToListAsync();
        return counts.ToDictionary(c => c.EventId, c => c.Count);
    }
}
