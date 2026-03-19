using DongThapHelpdesk.Api.Constants;
using DongThapHelpdesk.Api.Extensions;
using DongThapHelpdesk.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DongThapHelpDesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.Citizen)]
public class CitizenController : ControllerBase
{
    private readonly TicketRepository _ticketRepo;
    private readonly PointHistoryRepository _pointHistoryRepo;
    private readonly UserRepository _userRepo;

    public CitizenController(
        TicketRepository ticketRepo,
        PointHistoryRepository pointHistoryRepo,
        UserRepository userRepo)
    {
        _ticketRepo = ticketRepo;
        _pointHistoryRepo = pointHistoryRepo;
        _userRepo = userRepo;
    }

    /// <summary>Lấy danh sách báo cáo của chính mình</summary>
    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var citizenId = User.GetUserId();
        var tickets = await _ticketRepo
            .GetByCitizenIdAsync(citizenId);
        return Ok(tickets);
    }

    /// <summary>Lấy thông tin điểm của bản thân</summary>
    [HttpGet("my-points")]
    public async Task<IActionResult> GetMyPoints()
    {
        var citizenId = User.GetUserId();
        var user = await _userRepo.GetByIdAsync(citizenId);

        if (user?.CitizenProfile == null)
            return NotFound("Không tìm thấy thông tin người dùng");

        return Ok(new
        {
            TotalPoints = user.CitizenProfile.TotalPoints,
            // Tổng điểm tích lũy

            CurrentMonthPoints = user.CitizenProfile.CurrentMonthPoints,
            // Điểm tháng này

            CurrentQuarterPoints = user.CitizenProfile.CurrentQuarterPoints,
            // Điểm quý này

            TotalTicketsSubmitted = user.CitizenProfile.TotalTicketsSubmitted,
            // Tổng số báo cáo đã gửi

            TotalTicketsApproved = user.CitizenProfile.TotalTicketsApproved,
            // Số báo cáo được duyệt

            TotalTicketsRejected = user.CitizenProfile.TotalTicketsRejected,
            // Số báo cáo bị từ chối

            ApprovalRate = user.CitizenProfile.TotalTicketsSubmitted > 0
                ? Math.Round(
                    (double)user.CitizenProfile.TotalTicketsApproved
                    / user.CitizenProfile.TotalTicketsSubmitted * 100, 1)
                : 0,
            // Tỷ lệ báo cáo hợp lệ (%)
        });
    }

    /// <summary>Lấy lịch sử điểm của bản thân</summary>
    [HttpGet("my-points/history")]
    public async Task<IActionResult> GetMyPointHistory()
    {
        var citizenId = User.GetUserId();
        var history = await _pointHistoryRepo
            .GetByCitizenIdAsync(citizenId);
        return Ok(history);
    }

    /// <summary>Lấy lịch sử điểm theo tháng</summary>
    [HttpGet("my-points/history/{year}/{month}")]
    public async Task<IActionResult> GetMyPointHistoryByMonth(
        int year, int month)
    {
        if (month < 1 || month > 12)
            return BadRequest("Tháng không hợp lệ");

        var citizenId = User.GetUserId();
        var history = await _pointHistoryRepo
            .GetByCitizenIdAsync(citizenId, month, year);
        return Ok(history);
    }

    /// <summary>
    /// Xem bảng xếp hạng tháng hiện tại
    /// Công khai - ai cũng xem được
    /// </summary>
    [HttpGet("leaderboard/monthly")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMonthlyLeaderboard()
    {
        var leaderboard = await _userRepo
            .GetMonthlyLeaderboardAsync(top: 10);

        return Ok(leaderboard.Select((u, index) => new
        {
            Rank = index + 1,
            // Xếp hạng

            FullName = u.FullName,
            // Tên người dân

            Ward = u.CitizenProfile!.Ward,
            // Phường/Xã

            District = u.CitizenProfile.District,
            // Quận/Huyện

            Points = u.CitizenProfile.CurrentMonthPoints,
            // Điểm tháng này

            TotalTicketsApproved =
                u.CitizenProfile.TotalTicketsApproved
            // Số báo cáo hợp lệ
        }));
    }

    /// <summary>
    /// Xem bảng xếp hạng quý hiện tại
    /// Công khai - ai cũng xem được
    /// </summary>
    [HttpGet("leaderboard/quarterly")]
    [AllowAnonymous]
    public async Task<IActionResult> GetQuarterlyLeaderboard()
    {
        var leaderboard = await _userRepo
            .GetQuarterlyLeaderboardAsync(top: 10);

        return Ok(leaderboard.Select((u, index) => new
        {
            Rank = index + 1,
            FullName = u.FullName,
            Ward = u.CitizenProfile!.Ward,
            District = u.CitizenProfile.District,
            Points = u.CitizenProfile.CurrentQuarterPoints,
            TotalTicketsApproved =
                u.CitizenProfile.TotalTicketsApproved
        }));
    }
}