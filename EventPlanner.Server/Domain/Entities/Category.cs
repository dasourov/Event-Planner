using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EventPlanner.Server.Domain.Entities;

public class Category
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}
