using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Infrastructure.Persistence;

namespace EventPlanner.Server.Infrastructure.Repositories;

public class MongoUserRepository : IUserRepository
{
    private readonly MongoDbContext _context;

    public MongoUserRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(string id)
    {
        return await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<User>> GetByIdsAsync(IEnumerable<string> ids)
    {
        var filter = Builders<User>.Filter.In(u => u.Id, ids);
        return await _context.Users.Find(filter).ToListAsync();
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.Find(u => u.Email == email).FirstOrDefaultAsync();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users.Find(u => u.Username == username).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(User user)
    {
        await _context.Users.InsertOneAsync(user);
    }

    public async Task UpdateAsync(User user)
    {
        await _context.Users.ReplaceOneAsync(u => u.Id == user.Id, user);
    }

    public async Task<List<User>> ListAsync()
    {
        return await _context.Users.Find(_ => true).ToListAsync();
    }
}
