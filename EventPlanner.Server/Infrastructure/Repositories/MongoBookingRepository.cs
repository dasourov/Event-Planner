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
        => await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id);

    public async Task<Booking?> GetByUserAndEventAsync(string userId, string eventId)
        => await _context.Bookings.FirstOrDefaultAsync(b => b.UserId == userId && b.EventId == eventId);

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
        => await _context.Bookings.Where(b => b.UserId == userId).ToListAsync();

    public async Task<List<Booking>> ListByEventAsync(string eventId)
        => await _context.Bookings.Where(b => b.EventId == eventId).ToListAsync();

    public async Task<int> CountByEventAsync(string eventId)
        => await _context.Bookings.CountAsync(b => b.EventId == eventId);

    public async Task<Dictionary<string, int>> GetAttendeeCountsAsync(IEnumerable<string> eventIds)
    {
        var bookings = await _context.Bookings
            .Where(b => eventIds.Contains(b.EventId))
            .ToListAsync();

        var dict = bookings
            .GroupBy(b => b.EventId)
            .ToDictionary(g => g.Key, g => g.Count());

        // Ensure every requested eventId has a value (default 0)
        foreach (var id in eventIds)
        {
            if (!dict.ContainsKey(id))
            {
                dict[id] = 0;
            }
        }

        return dict;
    }
}
