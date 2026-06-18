using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Infrastructure.Persistence;

namespace EventPlanner.Server.Infrastructure.Repositories;

public class MongoCategoryRepository : ICategoryRepository
{
    private readonly MongoDbContext _context;

    public MongoCategoryRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(string id)
        => await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);

    public async Task<List<Category>> GetByIdsAsync(IEnumerable<string> ids)
        => await _context.Categories.Where(c => ids.Contains(c.Id)).ToListAsync();

    public async Task<Category?> GetByNameAsync(string name)
        => await _context.Categories.FirstOrDefaultAsync(c => c.Name == name);

    public async Task CreateAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Category>> ListAsync()
        => await _context.Categories.ToListAsync();
}
