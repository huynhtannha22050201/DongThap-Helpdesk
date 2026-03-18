using DongThapHelpdesk.Api.Attributes;
using DongThapHelpdesk.Api.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonCollection("notifications")]
public class Notification
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string RecipientId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? TicketId { get; set; }

    public NotificationType Type { get; set; }
    public string Message { get; set; }
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}