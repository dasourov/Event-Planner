using System.Collections.Generic;
using System.Threading.Tasks;
using EventPlanner.Server.Domain.Entities;

namespace EventPlanner.Server.Infrastructure.Repositories;

public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(string id);
    Task CreateAsync(Comment comment);
    Task UpdateAsync(Comment comment);
    Task DeleteAsync(string id);
    Task<List<Comment>> ListByEventAsync(string eventId);
}
