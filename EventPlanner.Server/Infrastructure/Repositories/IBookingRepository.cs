using System.Collections.Generic;
using System.Threading.Tasks;
using EventPlanner.Server.Domain.Entities;

namespace EventPlanner.Server.Infrastructure.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(string id);
    Task<Booking?> GetByUserAndEventAsync(string userId, string eventId);
    Task CreateAsync(Booking booking);
    Task DeleteAsync(string id);
    Task<List<Booking>> ListByUserAsync(string userId);
    Task<List<Booking>> ListByEventAsync(string eventId);
    Task<int> CountByEventAsync(string eventId);
}
