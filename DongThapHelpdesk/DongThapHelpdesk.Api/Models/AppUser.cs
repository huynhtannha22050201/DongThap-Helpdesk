using DongThapHelpdesk.Api.Enums;
using DongThapHelpdesk.Api.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DongThapHelpdesk.Api.Models
{
    [BsonCollection("users")]
    public class AppUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? DepartmentId { get; set; } // null for Citizen/Manager

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public CitizenProfile? CitizenProfile { get; set; } 
    }

    // ── Embedded: Thông tin Citizen ───────────────────────────────
    public class CitizenProfile
    {
        public string? Address { get; set; }
        // Địa chỉ thường trú của người dân
        // Ví dụ: "123 Nguyễn Huệ, Phường 1, TP. Cao Lãnh"

        public string? Ward { get; set; }
        // Phường/Xã
        // Ví dụ: "Phường 1"

        public string? District { get; set; }
        // Quận/Huyện
        // Ví dụ: "TP. Cao Lãnh"

        // ── Hệ thống điểm ────────────────────────────────────────
        public int TotalPoints { get; set; } = 0;
        // Tổng điểm tích lũy từ trước đến nay

        public int CurrentMonthPoints { get; set; } = 0;
        // Điểm trong tháng hiện tại

        public int CurrentQuarterPoints { get; set; } = 0;
        // Điểm trong quý hiện tại

        public int TotalTicketsSubmitted { get; set; } = 0;
        // Tổng số báo cáo đã gửi

        public int TotalTicketsApproved { get; set; } = 0;
        // Tổng số báo cáo được duyệt (hợp lệ)

        public int TotalTicketsRejected { get; set; } = 0;
        // Tổng số báo cáo bị từ chối (spam)

        public DateTime? LastActivityAt { get; set; }
        // Thời điểm gửi báo cáo gần nhất
    }
}
