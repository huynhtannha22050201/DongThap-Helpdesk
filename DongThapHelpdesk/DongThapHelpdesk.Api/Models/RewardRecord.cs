using DongThapHelpdesk.Api.Attributes;
using DongThapHelpdesk.Api.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DongThapHelpDesk.Api.Models;

[BsonCollection("reward_records")]
public class RewardRecord
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = null!;

    [BsonRepresentation(BsonType.ObjectId)]
    public string CitizenId { get; set; } = null!;
    // ID người dân được khen thưởng

    public RewardPreiod Period { get; set; }
    // Kỳ khen thưởng: tháng hoặc quý

    public int Year { get; set; }
    // Năm khen thưởng
    // Ví dụ: 2026

    public int PeriodNumber { get; set; }
    // Số thứ tự tháng hoặc quý
    // Tháng: 1-12 / Quý: 1-4

    public int TotalPoints { get; set; }
    // Tổng điểm trong kỳ

    public int Rank { get; set; }
    // Xếp hạng trong kỳ
    // Ví dụ: 1 = Nhất, 2 = Nhì, 3 = Ba

    public string RewardTitle { get; set; } = null!;
    // Danh hiệu khen thưởng
    // Ví dụ: "Công dân tích cực tháng 3/2026"

    public string? RewardDescription { get; set; }
    // Mô tả phần thưởng
    // Ví dụ: "Giấy khen + Phần quà trị giá 500.000đ"

    public bool IsAcknowledged { get; set; } = false;
    // true = người dân đã xem thông báo khen thưởng

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // Thời điểm tạo bản ghi khen thưởng
}