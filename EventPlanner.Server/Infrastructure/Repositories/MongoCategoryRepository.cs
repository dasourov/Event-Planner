using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
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
    {
        return await _context.Categories.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        return await _context.Categories.Find(c => c.Name == name).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Category category)
    {
        await _context.Categories.InsertOneAsync(category);
    }

    public async Task UpdateAsync(Category category)
    {
        await _context.Categories.ReplaceOneAsync(c => c.Id == category.Id, category);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Categories.DeleteOneAsync(c => c.Id == id);
    }

    public async Task<List<Category>> ListAsync()
    {
        return await _context.Categories.Find(_ => true).ToListAsync();
    }
}
