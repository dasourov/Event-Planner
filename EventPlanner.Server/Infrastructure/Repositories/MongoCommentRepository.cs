using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Infrastructure.Persistence;

namespace EventPlanner.Server.Infrastructure.Repositories;

public class MongoCommentRepository : ICommentRepository
{
    private readonly MongoDbContext _context;

    public MongoCommentRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetByIdAsync(string id)
    {
        return await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task CreateAsync(Comment comment)
    {
        await _context.Comments.AddAsync(comment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Comment comment)
    {
        _context.Comments.Update(comment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(string id)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
        if (comment != null)
        {
            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteManyAsync(List<string> ids)
    {
        var comments = await _context.Comments.Where(c => ids.Contains(c.Id)).ToListAsync();
        _context.Comments.RemoveRange(comments);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Comment>> ListByEventAsync(string eventId)
    {
        return await _context.Comments.Where(c => c.EventId == eventId).ToListAsync();
    }
}
