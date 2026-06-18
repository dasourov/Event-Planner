using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventPlanner.Server.Domain.Entities;

public class Comment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    [BsonRepresentation(BsonType.ObjectId)]
    public string EventId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    // Null for top-level comments; set when this comment is a reply to another comment
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentCommentId { get; set; }

    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
