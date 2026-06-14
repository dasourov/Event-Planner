using System.Collections.Generic;
using System.Threading.Tasks;
using EventPlanner.Server.Domain.Entities;

namespace EventPlanner.Server.Infrastructure.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id);
    Task<List<User>> GetByIdsAsync(List<string> ids);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task CreateAsync(User user);
    Task UpdateAsync(User user);
    Task<List<User>> ListAsync();
}
