using DongThapHelpdesk.Api.Attributes;
using DongThapHelpdesk.Api.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DongThapHelpdesk.Api.Models;

[BsonCollection("point_histories")]
public class PointHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string CitizenId { get; set; } = null!;
    // ID của người dân được cộng/trừ điểm

    [BsonRepresentation(BsonType.ObjectId)]
    public string? TicketId { get; set; }
    // ID của ticket liên quan
    // Null nếu điểm thưởng không liên quan ticket cụ thể

    public PointAction Action { get; set; }
    // Lý do cộng/trừ điểm

    public int Points { get; set; }
    // Số điểm thay đổi
    // Ví dụ: +10 hoặc -5

    public int BalanceAfter { get; set; }
    // Tổng điểm sau khi cộng/trừ
    // Dùng để hiển thị lịch sử điểm cho người dân

    public string? Note { get; set; }
    // Ghi chú
    // Ví dụ: "Báo cáo ổ gà đường Nguyễn Huệ được duyệt"

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Thời điểm điểm được cộng/trừ
}