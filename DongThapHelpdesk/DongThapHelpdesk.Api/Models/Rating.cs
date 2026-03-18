using DongThapHelpdesk.Api.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.ComponentModel.DataAnnotations;

[BsonCollection("ratings")]
public class Rating
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string TicketId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? CitizenId { get; set; } // null if anonymous

    [Range(1, 5)]
    public int Stars { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}