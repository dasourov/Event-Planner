using System.Collections.Generic;
using System.Threading.Tasks;
using EventPlanner.Server.Domain.Entities;

namespace EventPlanner.Server.Infrastructure.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(string id);
    Task<Category?> GetByNameAsync(string name);
    Task CreateAsync(Category category);
    Task UpdateAsync(Category category);
    Task DeleteAsync(string id);
    Task<List<Category>> ListAsync();
}
