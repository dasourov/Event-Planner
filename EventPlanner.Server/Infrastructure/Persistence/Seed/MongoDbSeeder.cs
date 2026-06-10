using System;
using System.Threading.Tasks;
using MongoDB.Driver;
using EventPlanner.Server.Domain.Entities;
using EventPlanner.Server.Domain.Enums;
using EventPlanner.Server.Infrastructure.Auth;

namespace EventPlanner.Server.Infrastructure.Persistence.Seed;

public class MongoDbSeeder
{
    private readonly MongoDbContext _context;
    private readonly PasswordHasher _passwordHasher;

    public MongoDbSeeder(MongoDbContext context, PasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task SeedAsync()
    {
        // 1. Seed Categories
        var categoryCount = await _context.Categories.CountDocumentsAsync(_ => true);
        if (categoryCount == 0)
        {
            var categories = new[]
            {
                new Category { Name = "Technology", Description = "Tech conferences, workshops, and meetups" },
                new Category { Name = "Sports", Description = "Athletic events, games, and outdoor activities" },
                new Category { Name = "Music", Description = "Concerts, festivals, and musical performances" },
                new Category { Name = "Art", Description = "Exhibitions, galleries, and creative workshops" }
            };
            await _context.Categories.InsertManyAsync(categories);
        }

        // 2. Seed Users (Admin and standard User)
        var userCount = await _context.Users.CountDocumentsAsync(_ => true);
        if (userCount == 0)
        {
            var admin = new User
            {
                Username = "admin",
                Email = "admin@eventplanner.com",
                PasswordHash = _passwordHasher.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                IsBanned = false,
                CreatedAt = DateTime.UtcNow
            };

            var user = new User
            {
                Username = "user",
                Email = "user@eventplanner.com",
                PasswordHash = _passwordHasher.HashPassword("User123!"),
                Role = UserRole.User,
                IsBanned = false,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Users.InsertManyAsync(new[] { admin, user });
        }
    }
}
