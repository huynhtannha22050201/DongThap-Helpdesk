using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using DongThapHelpdesk.Api.Attributes;
using DongThapHelpdesk.Api.Enums;

namespace DongThapHelpdesk.Api.Models;

[BsonCollection("tickets")]
public class Ticket
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string TicketCode { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public TicketStatus Status { get; set; } = TicketStatus.New;
    public TicketPriority Priority { get; set; } = TicketPriority.Normal;

    [BsonRepresentation(BsonType.ObjectId)]
    public string? CategoryId { get; set; }

    public GeoLocation? Location { get; set; }
    public List<Attachment> Attachments { get; set; } = new();

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

// ── GeoJSON chuẩn MongoDB ─────────────────────────────────
public class GeoLocation
{
    [BsonElement("type")]
    public string Type { get; set; } = "Point";
    // Luôn là "Point" vì đây là một điểm tọa độ

    [BsonElement("coordinates")]
    public double[] Coordinates { get; set; } = new double[2];
    // Theo chuẩn GeoJSON: [longitude, latitude]
    // Ví dụ: [105.9100, 10.3397]
    // QUAN TRỌNG: longitude trước, latitude sau

    [BsonElement("address")]
    public string? Address { get; set; }
    // Địa chỉ văn bản
    // Ví dụ: "Đường Nguyễn Huệ, TP. Cao Lãnh, Đồng Tháp"

    // Helper properties để code dễ đọc hơn
    [BsonIgnore]
    public double Longitude => Coordinates[0];
    // Truy cập kinh độ: location.Longitude

    [BsonIgnore]
    public double Latitude => Coordinates[1];
    // Truy cập vĩ độ: location.Latitude
}

// ── Attachment ────────────────────────────────────────────
public class Attachment
{
    public string FileName { get; set; } = null!;
    public string Url { get; set; } = null!;
    public string MimeType { get; set; } = null!;
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}