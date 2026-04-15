using DongThapHelpdesk.Api.Attributes;
using DongThapHelpdesk.Api.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DongThapHelpdesk.Api.Models;

[BsonCollection("departments")]
public class Department
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; } // e.g. "UBND-P1"
    public DepartmentLevel Level { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? ResponsibleUserId { get; set; } // default Assignee head

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}