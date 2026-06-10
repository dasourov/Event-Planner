using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using MongoDB.Driver;
using FluentValidation;
using MediatR;
using EventPlanner.Server.Common.Endpoints;
using EventPlanner.Server.Common.Behaviors;
using EventPlanner.Server.Infrastructure.Persistence;
using EventPlanner.Server.Infrastructure.Persistence.Seed;
using EventPlanner.Server.Infrastructure.Repositories;
using EventPlanner.Server.Infrastructure.Auth;
using EventPlanner.Server.Infrastructure.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Add SignalR
builder.Services.AddSignalR();

// Bind Settings
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

// MongoDB Client
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("mongodb");
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = builder.Configuration["MongoDb:ConnectionString"];
    }
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = "mongodb://localhost:27017";
    }
    return new MongoClient(connectionString);
});

// MongoDbContext
builder.Services.AddSingleton<MongoDbContext>();

// Repositories
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
builder.Services.AddScoped<IEventRepository, MongoEventRepository>();
builder.Services.AddScoped<IBookingRepository, MongoBookingRepository>();
builder.Services.AddScoped<ICommentRepository, MongoCommentRepository>();
builder.Services.AddScoped<ICategoryRepository, MongoCategoryRepository>();

// Services
builder.Services.AddSingleton<PasswordHasher>();
builder.Services.AddSingleton<JwtTokenService>();
builder.Services.AddScoped<MongoDbSeeder>();

// MediatR & FluentValidation
var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(cfg =>
{
    cfg.RegisterServicesFromAssembly(assembly);
    
    // Register Behaviors (in order of execution)
    cfg.AddOpenBehavior(typeof(ExceptionHandlingBehavior<,>));
    cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));
    cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
});

builder.Services.AddValidatorsFromAssembly(assembly);

// Add Endpoints scanning
builder.Services.AddEndpoints();

// Configure JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] ?? "super_secret_key_that_is_at_least_32_characters_long_12345!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "EventPlanner";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "EventPlanner";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret))
    };

    // Support token in query string for SignalR CommentHub
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs/comments"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthentication();
app.UseAuthorization();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<MongoDbSeeder>();
    try
    {
        await seeder.SeedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database");
    }
}

// Map SignalR Hubs
app.MapHub<CommentHub>("/hubs/comments");

// Map Scan Endpoints
app.MapEndpoints();

app.MapDefaultEndpoints();

// Map Root Welcome and Info Endpoint
app.MapGet("/", () => Results.Ok(new 
{
    Message = "EventPlanner API is running successfully!",
    DocumentationUrl = "/openapi/v1.json",
    AuthEndpoints = new[] { "POST /api/auth/register", "POST /api/auth/login", "GET /api/auth/me" },
    EventEndpoints = new[] { "GET /api/events", "POST /api/events", "GET /api/events/{id}", "PUT /api/events/{id}", "DELETE /api/events/{id}" },
    BookingEndpoints = new[] { "POST /api/bookings/{eventId}/join", "DELETE /api/bookings/{eventId}/leave", "GET /api/bookings/my" }
}));

app.UseFileServer();

app.Run();

public partial class Program { }
