using System.Collections.Generic;
using System.Threading.Tasks;
using EventPlanner.Server.Domain.Entities;

namespace EventPlanner.Server.Infrastructure.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(string id);
    Task<List<Event>> GetByIdsAsync(List<string> ids);
    Task CreateAsync(Event @event);
    Task UpdateAsync(Event @event);
    Task DeleteAsync(string id);
    Task<List<Event>> ListAsync(string? categoryId = null, string? searchTerm = null);
    Task<List<Event>> ListByOrganizerAsync(string organizerId);
    Task<List<Event>> ListPublishedAsync();
}
