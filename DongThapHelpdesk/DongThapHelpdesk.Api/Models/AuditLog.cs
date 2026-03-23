using DongThapHelpdesk.Api.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DongThapHelpdesk.Api.Models;

[BsonCollection("audit_logs")]
public class AuditLog
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? ActorId { get; set; }

    public string EntityType { get; set; } // "Ticket", "Department", "User"...
    public string EntityId { get; set; }
    public string Action { get; set; }     // "StatusChanged", "UserCreated"...
    public string? OldValue { get; set; }  // JSON snapshot
    public string? NewValue { get; set; }  // JSON snapshot
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}