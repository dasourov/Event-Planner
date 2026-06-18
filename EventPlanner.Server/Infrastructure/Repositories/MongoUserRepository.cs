using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
        => await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<List<User>> GetByIdsAsync(IEnumerable<string> ids)
        => await _context.Users.Where(u => ids.Contains(u.Id)).ToListAsync();

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

    public async Task CreateAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }

    public async Task<List<User>> ListAsync()
        => await _context.Users.ToListAsync();
}
