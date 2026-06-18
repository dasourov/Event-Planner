using System.Collections.Generic;
using System.Threading.Tasks;
using EventPlanner.Server.Domain.Entities;

namespace EventPlanner.Server.Infrastructure.Repositories;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(string id);
    Task<List<Event>> GetByIdsAsync(IEnumerable<string> ids);
    Task CreateAsync(Event @event);
    Task UpdateAsync(Event @event);
    Task DeleteAsync(string id);

    Task<(List<Event> Events, int TotalCount)> ListAsync(
        string? categoryId = null,
        string? searchTerm = null,
        string? status = null,
        string? organizerId = null,
        int page = 1,
        int pageSize = 20
    );

    Task<List<Event>> ListByOrganizerAsync(string organizerId);
    Task<List<Event>> ListPublishedAsync();
}