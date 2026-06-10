using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventPlanner.Server.Domain.Entities;

public class Booking
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonRepresentation(BsonType.ObjectId)]
    public string EventId { get; set; } = string.Empty;

    public DateTime BookedAt { get; set; } = DateTime.UtcNow;
}
