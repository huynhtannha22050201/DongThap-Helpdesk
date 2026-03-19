namespace DongThapHelpdesk.Api.Enums;

public enum PointAction
{
    TicketApproved,
    // Báo cáo được duyệt hợp lệ → +10 điểm

    TicketClosed,
    // Báo cáo được xử lý xong và đóng → +5 điểm

    TicketRejected,
    // Báo cáo bị từ chối spam → -5 điểm

    RatingSubmitted,
    // Đã đánh giá kết quả xử lý → +2 điểm

    FirstTicketOfMonth,
    // Báo cáo đầu tiên trong tháng → +5 điểm thưởng

    ConsecutiveDaysActive,
    // Gửi báo cáo nhiều ngày liên tiếp → +3 điểm
}