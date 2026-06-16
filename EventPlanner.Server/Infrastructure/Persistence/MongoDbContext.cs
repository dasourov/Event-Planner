using Microsoft.EntityFrameworkCore;
using MongoDB.EntityFrameworkCore.Extensions;
using EventPlanner.Server.Domain.Entities;

namespace EventPlanner.Server.Infrastructure.Persistence;

public class MongoDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Event> Events { get; set; } = null!;
    public DbSet<Booking> Bookings { get; set; } = null!;
    public DbSet<Comment> Comments { get; set; } = null!;
    public DbSet<Category> Categories { get; set; } = null!;

    public MongoDbContext(DbContextOptions<MongoDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>().ToCollection("Users");
        modelBuilder.Entity<Event>().ToCollection("Events");
        modelBuilder.Entity<Booking>().ToCollection("Bookings");
        modelBuilder.Entity<Comment>().ToCollection("Comments");
        modelBuilder.Entity<Category>().ToCollection("Categories");
    }
}
