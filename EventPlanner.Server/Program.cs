using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using FluentValidation;
using MediatR;
using EventPlanner.Server.Common.Endpoints;
using EventPlanner.Server.Common.Behaviors;
using EventPlanner.Server.Common.Errors;
using Scalar.AspNetCore;
using EventPlanner.Server.Infrastructure.Persistence;
using EventPlanner.Server.Infrastructure.Persistence.Seed;
using EventPlanner.Server.Infrastructure.Repositories;
using EventPlanner.Server.Infrastructure.Auth;
using EventPlanner.Server.Infrastructure.SignalR;

EnvLoader.Load();

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Add SignalR with camelCase JSON payloads for the React client
builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        options.PayloadSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Bind Settings
builder.Services.Configure<MongoDbSettings>(builder.Configuration.GetSection("MongoDb"));
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
// Configure MongoDB Context (EF Core)
builder.Services.AddDbContext<MongoDbContext>((sp, options) =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();

    var connectionString =
        configuration.GetConnectionString("mongodb") ??
        configuration.GetConnectionString("eventplanner") ??
        Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ??
        configuration["MongoDb:ConnectionString"];

    if (string.IsNullOrEmpty(connectionString) ||
        connectionString == "MONGODB_CONNECTION_STRING_PLACEHOLDER" ||
        connectionString == "your_mongodb_connection_string_here")
    {
        connectionString = "mongodb://localhost:27017";
    }

    var dbName = configuration["MongoDb:DatabaseName"] ?? "eventplanner";

    options.UseMongoDB(connectionString, dbName);
});

// Repositories
builder.Services.AddScoped<IUserRepository, MongoUserRepository>();
builder.Services.AddScoped<IEventRepository, MongoEventRepository>();
builder.Services.AddScoped<IBookingRepository, MongoBookingRepository>();
builder.Services.AddScoped<ICommentRepository, MongoCommentRepository>();
builder.Services.AddScoped<ICategoryRepository, MongoCategoryRepository>();

// Register native MongoDB client for direct driver access (used by seeder)
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var connectionString =
        configuration.GetConnectionString("mongodb") ??
        configuration.GetConnectionString("eventplanner") ??
        Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING") ??
        configuration["MongoDb:ConnectionString"] ??
        "mongodb://localhost:27017";
    return new MongoClient(connectionString);
});

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
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET");
if (string.IsNullOrEmpty(jwtSecret))
{
    jwtSecret = builder.Configuration["Jwt:Secret"] ?? "super_secret_key_that_is_at_least_32_characters_long_12345!";
}
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
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseMiddleware<BannedUserMiddleware>();
app.UseAuthorization();

app.UseStaticFiles();
app.UseFileServer();

// Seed Database
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<MongoDbSeeder>();
    var seedLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        seedLogger.LogInformation("[Seeder] Starting database seed...");
        await seeder.SeedAsync();
        seedLogger.LogInformation("[Seeder] Database seed completed successfully.");
    }
    catch (Exception ex)
    {
        seedLogger.LogError(ex, "[Seeder] An error occurred while initializing the database");
        if (app.Environment.IsDevelopment())
            throw; // Surface the error in dev so it's not silently swallowed
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
    Message = "EventPlanner API v1 is running successfully!",
    DocumentationUrl = "/openapi/v1.json",
    AuthEndpoints = new[] { "POST /api/v1/auth/register", "POST /api/v1/auth/login", "GET /api/v1/auth/me" },
    EventEndpoints = new[] { "GET /api/v1/events", "POST /api/v1/events", "GET /api/v1/events/{id}", "PUT /api/v1/events/{id}", "DELETE /api/v1/events/{id}" },
    BookingEndpoints = new[] { "POST /api/v1/bookings/{eventId}/join", "DELETE /api/v1/bookings/{eventId}/leave", "GET /api/v1/bookings/my" }
}));

var webRoot = app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot");
var indexFile = Path.Combine(webRoot, "index.html");
if (File.Exists(indexFile))
{
    app.MapFallbackToFile("index.html");
}

app.Run();

public partial class Program { }

public static class EnvLoader
{
    public static void Load()
    {
        var dir = new System.IO.DirectoryInfo(System.IO.Directory.GetCurrentDirectory());
        while (dir != null)
        {
            var envFile = System.IO.Path.Combine(dir.FullName, ".env");
            if (System.IO.File.Exists(envFile))
            {
                foreach (var line in System.IO.File.ReadAllLines(envFile))
                {
                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#")) continue;
                    var parts = line.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var val = parts[1].Trim().Trim('"').Trim('\'');
                        System.Environment.SetEnvironmentVariable(key, val);
                    }
                }
                break;
            }
            dir = dir.Parent;
        }
    }
}
