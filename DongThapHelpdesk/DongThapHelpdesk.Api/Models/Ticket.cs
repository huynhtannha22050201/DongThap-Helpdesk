using DongThapHelpdesk.Api.Attributes;
using DongThapHelpdesk.Api.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonCollection("tickets")]
public class Ticket
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string TicketCode { get; set; } // e.g. PA-032026-001

    public string Title { get; set; }
    public string Description { get; set; }
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;

    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; }

    public GeoLocation? Location { get; set; }            // embedded
    public List<Attachment> Attachments { get; set; } = new(); // embedded

    // Reporter: either AppUser.Id (registered) or anonymous info
    [BsonRepresentation(BsonType.ObjectId)]
    public string? ReporterId { get; set; }
    public string? AnonymousName { get; set; }
    public string? AnonymousPhone { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? AssignedDepartmentId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? AssignedUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SlaDeadline { get; set; }
    public bool IsSlaBreached { get; set; } = false;
    public DateTime? ClosedAt { get; set; }
    public string? RejectionReason { get; set; }
}

// --- Embedded value objects ---
public class GeoLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? Address { get; set; }
}

public class Attachment
{
    public string FileName { get; set; }
    public string Url { get; set; }
    public string MimeType { get; set; } // image/jpeg, video/mp4, audio/mpeg
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}