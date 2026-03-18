using DongThapHelpdesk.Api.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonCollection("incident_categories")]
public class IncidentCategory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public string Name { get; set; }
    public string Code { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string? ParentCategoryId { get; set; }

    public int DefaultSlaHours { get; set; } = 72;
    public bool IsActive { get; set; } = true;
}