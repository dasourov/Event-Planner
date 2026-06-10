using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
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
        return await _context.Comments.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task CreateAsync(Comment comment)
    {
        await _context.Comments.InsertOneAsync(comment);
    }

    public async Task UpdateAsync(Comment comment)
    {
        await _context.Comments.ReplaceOneAsync(c => c.Id == comment.Id, comment);
    }

    public async Task DeleteAsync(string id)
    {
        await _context.Comments.DeleteOneAsync(c => c.Id == id);
    }

    public async Task<List<Comment>> ListByEventAsync(string eventId)
    {
        return await _context.Comments.Find(c => c.EventId == eventId).ToListAsync();
    }
}
