using DongThapHelpdesk.Api.Attributes;
using DongThapHelpdesk.Api.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DongThapHelpDesk.Api.Models;

[BsonCollection("point_policies")]
public class PointPolicy
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    public string Name { get; set; } = null!;
    // Tên chính sách
    // Ví dụ: "Gửi báo cáo hợp lệ"

    public PointAction Action { get; set; }
    // Loại hành động được tính điểm

    public int Points { get; set; }
    // Số điểm được cộng hoặc trừ
    // Cộng: điểm dương
    // Trừ: điểm âm

    public string? Description { get; set; }
    // Mô tả chi tiết
    // Ví dụ: "Cộng 10 điểm khi báo cáo được xác nhận hợp lệ"

    public bool IsActive { get; set; } = true;
    // true = đang áp dụng chính sách này
}