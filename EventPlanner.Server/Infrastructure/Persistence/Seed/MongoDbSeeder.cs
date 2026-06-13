using System;
using System.Collections.Generic;
using System.Linq;
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
        var categories = await SeedCategoriesAsync();
        var users = await SeedUsersAsync();
        var events = await SeedEventsAsync(categories, users);
        await SeedBookingsAsync(events, users);
        await SeedCommentsAsync(events, users);
    }

    private async Task<List<Category>> SeedCategoriesAsync()
    {
        var desired = new List<Category>
        {
            new() { Name = "Technology", Description = "Tech conferences, workshops, and meetups" },
            new() { Name = "Sports", Description = "Athletic events, games, and outdoor activities" },
            new() { Name = "Music", Description = "Concerts, festivals, and musical performances" },
            new() { Name = "Art", Description = "Exhibitions, galleries, and creative workshops" },
            new() { Name = "Networking", Description = "Professional networking and career events" },
            new() { Name = "Wellness", Description = "Yoga, meditation, and mindfulness gatherings" },
            new() { Name = "Workshop", Description = "Hands-on learning and skill-building sessions" },
            new() { Name = "Food", Description = "Culinary experiences, tastings, and cooking classes" }
        };

        var existing = await _context.Categories.Find(_ => true).ToListAsync();
        var existingNames = existing.Select(c => c.Name).ToHashSet();
        var toInsert = desired.Where(d => !existingNames.Contains(d.Name)).ToList();

        if (toInsert.Count > 0)
        {
            await _context.Categories.InsertManyAsync(toInsert);
        }

        return await _context.Categories.Find(_ => true).ToListAsync();
    }

    private async Task<List<User>> SeedUsersAsync()
    {
        var now = DateTime.UtcNow;
        var desired = new List<User>
        {
            new()
            {
                Username = "admin",
                Email = "admin@eventplanner.com",
                PasswordHash = _passwordHasher.HashPassword("Admin123!"),
                Role = UserRole.Admin,
                IsBanned = false,
                CreatedAt = now
            },
            new()
            {
                Username = "user",
                Email = "user@eventplanner.com",
                PasswordHash = _passwordHasher.HashPassword("User123!"),
                Role = UserRole.User,
                IsBanned = false,
                CreatedAt = now
            },
            new()
            {
                Username = "alice",
                Email = "alice@eventplanner.com",
                PasswordHash = _passwordHasher.HashPassword("Alice123!"),
                Role = UserRole.User,
                IsBanned = false,
                CreatedAt = now
            },
            new()
            {
                Username = "bob",
                Email = "bob@eventplanner.com",
                PasswordHash = _passwordHasher.HashPassword("Bob12345!"),
                Role = UserRole.User,
                IsBanned = false,
                CreatedAt = now
            },
            new()
            {
                Username = "charlie",
                Email = "charlie@eventplanner.com",
                PasswordHash = _passwordHasher.HashPassword("Charlie123!"),
                Role = UserRole.User,
                IsBanned = false,
                CreatedAt = now
            },
            new()
            {
                Username = "diana",
                Email = "diana@eventplanner.com",
                PasswordHash = _passwordHasher.HashPassword("Diana123!"),
                Role = UserRole.User,
                IsBanned = true,
                CreatedAt = now
            }
        };

        var existing = await _context.Users.Find(_ => true).ToListAsync();
        var existingEmails = existing.Select(u => u.Email).ToHashSet();
        var toInsert = desired.Where(d => !existingEmails.Contains(d.Email)).ToList();

        if (toInsert.Count > 0)
        {
            await _context.Users.InsertManyAsync(toInsert);
        }

        return await _context.Users.Find(_ => true).ToListAsync();
    }

    private async Task<List<Event>> SeedEventsAsync(List<Category> categories, List<User> users)
    {
        var count = await _context.Events.CountDocumentsAsync(_ => true);
        if (count > 0)
        {
            return await _context.Events.Find(_ => true).ToListAsync();
        }

        var tech = categories.First(c => c.Name == "Technology");
        var sports = categories.First(c => c.Name == "Sports");
        var music = categories.First(c => c.Name == "Music");
        var art = categories.First(c => c.Name == "Art");
        var networking = categories.First(c => c.Name == "Networking");
        var wellness = categories.First(c => c.Name == "Wellness");
        var workshop = categories.First(c => c.Name == "Workshop");

        var admin = users.First(u => u.Username == "admin");
        var alice = users.First(u => u.Username == "alice");
        var bob = users.First(u => u.Username == "bob");
        var charlie = users.First(u => u.Username == "charlie");
        var diana = users.First(u => u.Username == "diana");

        var now = DateTime.UtcNow;
        var events = new List<Event>
        {
            new()
            {
                Title = ".NET Aspire Developer Summit",
                Description = "Learn how to build cloud-native apps with .NET Aspire, MongoDB, and modern microservices patterns.",
                Location = "Munich Convention Center, Germany",
                Latitude = 48.1351,
                Longitude = 11.5820,
                Date = now.AddDays(30),
                OrganizerId = alice.Id,
                Status = EventStatus.Published,
                CategoryId = tech.Id,
                MaxAttendees = 200,
                CreatedAt = now
            },
            new()
            {
                Title = "Munich City Marathon 2026",
                Description = "Annual city marathon through Munich's most scenic routes. All skill levels welcome.",
                Location = "Olympiapark, Munich",
                Latitude = 48.1755,
                Longitude = 11.5518,
                Date = now.AddDays(45),
                OrganizerId = bob.Id,
                Status = EventStatus.Published,
                CategoryId = sports.Id,
                MaxAttendees = 5000,
                CreatedAt = now
            },
            new()
            {
                Title = "Live Jazz Night",
                Description = "An intimate evening with award-winning jazz performers in a candlelit venue.",
                Location = "Blue Note Club, Berlin",
                Latitude = 52.5200,
                Longitude = 13.4050,
                Date = now.AddDays(10),
                OrganizerId = charlie.Id,
                Status = EventStatus.Published,
                CategoryId = music.Id,
                MaxAttendees = 80,
                CreatedAt = now
            },
            new()
            {
                Title = "Modern Art Exhibition Opening",
                Description = "Showcasing emerging European contemporary artists. Free admission, refreshments served.",
                Location = "Pinakothek der Moderne, Munich",
                Latitude = 48.1497,
                Longitude = 11.5723,
                Date = now.AddDays(7),
                OrganizerId = alice.Id,
                Status = EventStatus.Published,
                CategoryId = art.Id,
                MaxAttendees = null,
                CreatedAt = now
            },
            new()
            {
                Title = "Startup Pitch Night",
                Description = "Founders pitch to investors. Network with VCs, angel investors, and fellow entrepreneurs.",
                Location = "WeWork, Munich",
                Latitude = 48.1372,
                Longitude = 11.5755,
                Date = now.AddDays(21),
                OrganizerId = admin.Id,
                Status = EventStatus.Published,
                CategoryId = networking.Id,
                MaxAttendees = 100,
                CreatedAt = now
            },
            new()
            {
                Title = "Sunrise Yoga in the Park",
                Description = "Start your day with vinyasa flow yoga as the sun rises. Mats provided.",
                Location = "Englischer Garten, Munich",
                Latitude = 48.1530,
                Longitude = 11.5917,
                Date = now.AddDays(5),
                OrganizerId = diana.Id,
                Status = EventStatus.Published,
                CategoryId = wellness.Id,
                MaxAttendees = 30,
                CreatedAt = now
            },
            new()
            {
                Title = "React 19 Workshop",
                Description = "Deep dive into React 19's new features: actions, server components, and the use() hook.",
                Location = "Online (Zoom)",
                Latitude = null,
                Longitude = null,
                Date = now.AddDays(14),
                OrganizerId = bob.Id,
                Status = EventStatus.Published,
                CategoryId = workshop.Id,
                MaxAttendees = 50,
                CreatedAt = now
            },
            new()
            {
                Title = "Internal Team Hackathon (Draft)",
                Description = "Private hackathon for the engineering team. Not yet published.",
                Location = "TBD",
                Latitude = null,
                Longitude = null,
                Date = now.AddDays(60),
                OrganizerId = alice.Id,
                Status = EventStatus.Draft,
                CategoryId = tech.Id,
                MaxAttendees = 40,
                CreatedAt = now
            },
            new()
            {
                Title = "Spring Music Festival",
                Description = "Three-day outdoor music festival featuring 30+ artists. Cancelled due to venue issues.",
                Location = "Theresienwiese, Munich",
                Latitude = 48.1316,
                Longitude = 11.5495,
                Date = now.AddDays(40),
                OrganizerId = charlie.Id,
                Status = EventStatus.Cancelled,
                CategoryId = music.Id,
                MaxAttendees = 10000,
                CreatedAt = now
            },
            new()
            {
                Title = "Tech Career Fair",
                Description = "Connect with top tech companies hiring engineers, designers, and product managers.",
                Location = "TU Munich, Garching Campus",
                Latitude = 48.2628,
                Longitude = 11.6692,
                Date = now.AddDays(28),
                OrganizerId = admin.Id,
                Status = EventStatus.Published,
                CategoryId = networking.Id,
                MaxAttendees = 500,
                CreatedAt = now
            }
        };

        await _context.Events.InsertManyAsync(events);
        return events;
    }

    private async Task SeedBookingsAsync(List<Event> events, List<User> users)
    {
        var count = await _context.Bookings.CountDocumentsAsync(_ => true);
        if (count > 0) return;

        var user = users.First(u => u.Username == "user");
        var alice = users.First(u => u.Username == "alice");
        var bob = users.First(u => u.Username == "bob");
        var charlie = users.First(u => u.Username == "charlie");

        var published = events.Where(e => e.Status == EventStatus.Published).ToList();
        if (published.Count == 0) return;

        var now = DateTime.UtcNow;
        var bookings = new List<Booking>();

        // "user" books every published event — gives "My Bookings" page a populated list
        foreach (var ev in published)
        {
            bookings.Add(new Booking { UserId = user.Id, EventId = ev.Id, BookedAt = now });
        }

        // Other users sprinkle bookings across events to make attendee lists non-trivial
        if (published.Count >= 2)
        {
            bookings.Add(new Booking { UserId = alice.Id, EventId = published[1].Id, BookedAt = now });
        }
        if (published.Count >= 3)
        {
            bookings.Add(new Booking { UserId = bob.Id, EventId = published[2].Id, BookedAt = now });
            bookings.Add(new Booking { UserId = charlie.Id, EventId = published[2].Id, BookedAt = now });
        }
        if (published.Count >= 4)
        {
            bookings.Add(new Booking { UserId = charlie.Id, EventId = published[3].Id, BookedAt = now });
        }

        await _context.Bookings.InsertManyAsync(bookings);
    }

    private async Task SeedCommentsAsync(List<Event> events, List<User> users)
    {
        var count = await _context.Comments.CountDocumentsAsync(_ => true);
        if (count > 0) return;

        var user = users.First(u => u.Username == "user");
        var alice = users.First(u => u.Username == "alice");
        var bob = users.First(u => u.Username == "bob");
        var charlie = users.First(u => u.Username == "charlie");

        var published = events.Where(e => e.Status == EventStatus.Published).ToList();
        if (published.Count == 0) return;

        var now = DateTime.UtcNow;
        var comments = new List<Comment>();

        if (published.Count >= 1)
        {
            var ev = published[0];
            comments.Add(new Comment { EventId = ev.Id, UserId = user.Id, Content = "Looking forward to this!", CreatedAt = now.AddHours(-3) });
            comments.Add(new Comment { EventId = ev.Id, UserId = charlie.Id, Content = "Will the sessions be recorded?", CreatedAt = now.AddHours(-2) });
            comments.Add(new Comment { EventId = ev.Id, UserId = alice.Id, Content = "Yes, recordings will be available on-demand for attendees.", CreatedAt = now.AddHours(-1) });
        }

        if (published.Count >= 2)
        {
            var ev = published[1];
            comments.Add(new Comment { EventId = ev.Id, UserId = user.Id, Content = "Where is the start line exactly?", CreatedAt = now.AddHours(-5) });
            comments.Add(new Comment { EventId = ev.Id, UserId = bob.Id, Content = "Start is at the main entrance of Olympiapark.", CreatedAt = now.AddHours(-4) });
        }

        if (published.Count >= 3)
        {
            var ev = published[2];
            comments.Add(new Comment { EventId = ev.Id, UserId = charlie.Id, Content = "Can't wait for tonight!", CreatedAt = now.AddMinutes(-30) });
        }

        if (published.Count >= 4)
        {
            var ev = published[3];
            comments.Add(new Comment { EventId = ev.Id, UserId = alice.Id, Content = "Excited to showcase the new works tonight.", CreatedAt = now.AddHours(-8) });
            comments.Add(new Comment { EventId = ev.Id, UserId = user.Id, Content = "Will there be guided tours?", CreatedAt = now.AddHours(-2) });
        }

        await _context.Comments.InsertManyAsync(comments);
    }
}
