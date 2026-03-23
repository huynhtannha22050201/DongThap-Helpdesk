using DongThapHelpdesk.Api.Enums;
using DongThapHelpdesk.Api.Repositories;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Services;

public class PointService
{
    private readonly UserRepository _userRepo;
    private readonly PointHistoryRepository _pointHistoryRepo;

    // Bảng điểm mặc định
    private readonly Dictionary<PointAction, int> _pointTable = new()
    {
        { PointAction.TicketApproved,        10 },
        // Báo cáo hợp lệ được duyệt → +10 điểm

        { PointAction.TicketClosed,           5 },
        // Báo cáo xử lý xong → +5 điểm

        { PointAction.TicketRejected,        -5 },
        // Báo cáo spam bị từ chối → -5 điểm

        { PointAction.RatingSubmitted,        2 },
        // Đánh giá kết quả → +2 điểm

        { PointAction.FirstTicketOfMonth,     5 },
        // Báo cáo đầu tiên trong tháng → +5 điểm

        { PointAction.ConsecutiveDaysActive,  3 },
        // Hoạt động liên tiếp nhiều ngày → +3 điểm
    };

    public PointService(
        UserRepository userRepo,
        PointHistoryRepository pointHistoryRepo)
    {
        _userRepo = userRepo;
        _pointHistoryRepo = pointHistoryRepo;
    }

    public async Task AddPointsAsync(
        string citizenId,
        PointAction action,
        string? ticketId = null,
        string? note = null)
    {
        // Lấy số điểm theo hành động
        var points = _pointTable[action];

        // Lấy thông tin user
        var user = await _userRepo.GetByIdAsync(citizenId);
        if (user == null || user.CitizenProfile == null) return;

        // Cập nhật điểm
        user.CitizenProfile.TotalPoints += points;
        user.CitizenProfile.CurrentMonthPoints += points;
        user.CitizenProfile.CurrentQuarterPoints += points;
        user.CitizenProfile.LastActivityAt = DateTime.UtcNow;

        // Cập nhật số liệu báo cáo
        if (action == PointAction.TicketApproved)
            user.CitizenProfile.TotalTicketsApproved++;

        if (action == PointAction.TicketRejected)
            user.CitizenProfile.TotalTicketsRejected++;

        // Lưu lại user
        await _userRepo.UpdateAsync(user);

        // Ghi lịch sử điểm
        var history = new PointHistory
        {
            CitizenId = citizenId,
            TicketId = ticketId,
            Action = action,
            Points = points,
            BalanceAfter = user.CitizenProfile.TotalPoints,
            Note = note ?? GetDefaultNote(action),
            CreatedAt = DateTime.UtcNow
        };

        await _pointHistoryRepo.CreateAsync(history);
    }

    // Reset điểm tháng/quý — gọi bởi Background Service
    public async Task ResetMonthlyPointsAsync()
    {
        await _userRepo.ResetMonthlyPointsAsync();
    }

    public async Task ResetQuarterlyPointsAsync()
    {
        await _userRepo.ResetQuarterlyPointsAsync();
    }

    private static string GetDefaultNote(PointAction action)
        => action switch
        {
            PointAction.TicketApproved =>
                "Báo cáo được duyệt hợp lệ",
            PointAction.TicketClosed =>
                "Báo cáo được xử lý và đóng thành công",
            PointAction.TicketRejected =>
                "Báo cáo bị từ chối do không hợp lệ",
            PointAction.RatingSubmitted =>
                "Đã đánh giá kết quả xử lý",
            PointAction.FirstTicketOfMonth =>
                "Thưởng báo cáo đầu tiên trong tháng",
            PointAction.ConsecutiveDaysActive =>
                "Thưởng hoạt động tích cực liên tiếp",
            _ => ""
        };
}