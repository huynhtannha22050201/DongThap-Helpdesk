using DongThapHelpdesk.Api.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

[BsonCollection("holidays")]
public class Holiday
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; } // "Giỗ Tổ Hùng Vương"
    public bool IsRecurringYearly { get; set; } = true;
}