using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using EventPlanner.Server.Domain.Enums;

namespace EventPlanner.Server.Domain.Entities;

public class Event
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    
    public DateTime Date { get; set; }
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string OrganizerId { get; set; } = string.Empty;
    
    public EventStatus Status { get; set; } = EventStatus.Draft;
    
    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; } = string.Empty;
    
    public int? MaxAttendees { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
