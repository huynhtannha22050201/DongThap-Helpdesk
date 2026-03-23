using DongThapHelpdesk.Api.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DongThapHelpdesk.Api.Models;

[BsonCollection("sla_policies")]
public class SlaPolicy
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public string CategoryId { get; set; }

    public int ResolutionHours { get; set; }   // business hours to resolve
    public int WarningThresholdPercent { get; set; } = 80; // warn at 80% elapsed
    public bool ExcludeWeekends { get; set; } = true;
    public bool ExcludeHolidays { get; set; } = true;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}