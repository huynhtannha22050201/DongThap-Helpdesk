using DongThapHelpdesk.Api.Attributes;
using DongThapHelpdesk.Api.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DongThapHelpdesk.Api.Models;

[BsonCollection("ticket_activities")]
public class TicketActivity
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string TicketId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? ActorId { get; set; } // null = System

    public ActivityType ActionType { get; set; }
    public TicketStatus? OldStatus { get; set; }
    public TicketStatus? NewStatus { get; set; }
    public string? Comment { get; set; }
    public List<string> AttachmentUrls { get; set; } = new(); // result proof photos
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}