using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using EventPlanner.Server.Domain.Enums;

namespace EventPlanner.Server.Domain.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; } = UserRole.User;
    private bool? _isOrganizer = false;
    public bool? IsOrganizer
    {
        get => _isOrganizer ?? false;
        set => _isOrganizer = value;
    }
    public bool IsBanned { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
