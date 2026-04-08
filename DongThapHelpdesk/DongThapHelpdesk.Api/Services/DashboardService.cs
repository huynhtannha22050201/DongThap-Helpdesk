using DongThapHelpdesk.Api.Enums;
using DongThapHelpdesk.Api.Models;
using DongThapHelpdesk.Api.Repositories;
using MongoDB.Driver;

namespace DongThapHelpdesk.Api.Services;

public class DashboardService
{
    private readonly TicketRepository _ticketRepo;
    private readonly UserRepository _userRepo;
    private readonly CategoryRepository _categoryRepo;
    private readonly DepartmentRepository _departmentRepo;
    private readonly RatingRepository _ratingRepo;

    public DashboardService(
        TicketRepository ticketRepo,
        UserRepository userRepo,
        CategoryRepository categoryRepo,
        DepartmentRepository departmentRepo,
        RatingRepository ratingRepo)
    {
        _ticketRepo = ticketRepo;
        _userRepo = userRepo;
        _categoryRepo = categoryRepo;
        _departmentRepo = departmentRepo;
        _ratingRepo = ratingRepo;
    }

    // ══════════════════════════════════════════════════════
    // UC13: THỐNG KÊ TỔNG QUAN
    // ══════════════════════════════════════════════════════

    public async Task<object> GetStatsAsync()
    {
        var allTickets = await _ticketRepo.GetAllAsync();
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(
            now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var startOfYear = new DateTime(
            now.Year, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // ── Thống kê theo trạng thái ──────────────────────
        var byStatus = allTickets
            .GroupBy(t => t.Status)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Count());

        // ── Thống kê theo danh mục ────────────────────────
        var categories = await _categoryRepo.GetAllAsync();
        var categoryMap = categories
            .ToDictionary(c => c.Id, c => c.Name);

        var byCategory = allTickets
            .Where(t => t.CategoryId != null)
            .GroupBy(t => t.CategoryId!)
            .Select(g => new
            {
                CategoryId = g.Key,
                CategoryName = categoryMap
                    .GetValueOrDefault(g.Key, "Không xác định"),
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        // ── Thống kê theo đơn vị xử lý ───────────────────
        var departments = await _departmentRepo
            .GetActiveAsync();
        var deptMap = departments
            .ToDictionary(d => d.Id, d => d.Name);

        var byDepartment = allTickets
            .Where(t => t.AssignedDepartmentId != null)
            .GroupBy(t => t.AssignedDepartmentId!)
            .Select(g => new
            {
                DepartmentId = g.Key,
                DepartmentName = deptMap
                    .GetValueOrDefault(g.Key, "Không xác định"),
                Total = g.Count(),
                Closed = g.Count(t =>
                    t.Status == TicketStatus.Closed),
                InProgress = g.Count(t =>
                    t.Status == TicketStatus.InProgress),
                SlaBreached = g.Count(t => t.IsSlaBreached)
            })
            .OrderByDescending(x => x.Total)
            .ToList();

        // ── Thống kê theo mức ưu tiên ────────────────────
        var byPriority = allTickets
            .GroupBy(t => t.Priority)
            .ToDictionary(
                g => g.Key.ToString(),
                g => g.Count());

        // ── Thống kê tháng hiện tại ──────────────────────
        var thisMonth = allTickets
            .Where(t => t.CreatedAt >= startOfMonth)
            .ToList();

        // ── Thống kê năm hiện tại ────────────────────────
        var thisYear = allTickets
            .Where(t => t.CreatedAt >= startOfYear)
            .ToList();

        // ── Thời gian xử lý trung bình (giờ) ─────────────
        var closedTickets = allTickets
            .Where(t => t.Status == TicketStatus.Closed
                && t.ClosedAt != null)
            .ToList();

        var avgResolutionHours = closedTickets.Any()
            ? Math.Round(closedTickets
                .Average(t =>
                    (t.ClosedAt!.Value - t.CreatedAt)
                        .TotalHours), 1)
            : 0;

        // ── Tổng số người dân đã đăng ký ─────────────────
        var citizenFilter = Builders<AppUser>.Filter
            .Eq(u => u.Role, UserRole.Citizen);
        var citizens = await _userRepo
            .GetByFilterAsync(citizenFilter);

        return new
        {
            // ── Tổng quan ─────────────────────────────────
            TotalTickets = allTickets.Count,
            TotalThisMonth = thisMonth.Count,
            TotalThisYear = thisYear.Count,
            TotalClosed = allTickets
                .Count(t => t.Status == TicketStatus.Closed),
            TotalRejected = allTickets
                .Count(t => t.Status == TicketStatus.Rejected),
            TotalSlaBreached = allTickets
                .Count(t => t.IsSlaBreached),
            TotalCitizens = citizens.Count,

            // ── Tỷ lệ ────────────────────────────────────
            ClosedRate = allTickets.Count > 0
                ? Math.Round(
                    (double)allTickets
                        .Count(t => t.Status == TicketStatus.Closed)
                    / allTickets.Count * 100, 1)
                : 0,

            SlaComplianceRate = closedTickets.Count > 0
                ? Math.Round(
                    (double)closedTickets
                        .Count(t => !t.IsSlaBreached)
                    / closedTickets.Count * 100, 1)
                : 0,

            AvgResolutionHours = avgResolutionHours,

            // ── Chi tiết ──────────────────────────────────
            ByStatus = byStatus,
            ByCategory = byCategory,
            ByDepartment = byDepartment,
            ByPriority = byPriority,

            // ── Xu hướng 30 ngày ──────────────────────────
            DailyTrend = allTickets
                .Where(t => t.CreatedAt >= now.AddDays(-30))
                .GroupBy(t => t.CreatedAt.Date)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Date = g.Key.ToString("yyyy-MM-dd"),
                    Created = g.Count(),
                    Closed = g.Count(t =>
                        t.Status == TicketStatus.Closed),
                    SlaBreached = g.Count(t =>
                        t.IsSlaBreached)
                })
                .ToList(),

            // ── Xu hướng 12 tháng ─────────────────────────
            MonthlyTrend = allTickets
                .Where(t => t.CreatedAt >= now.AddMonths(-12))
                .GroupBy(t => new
                {
                    t.CreatedAt.Year,
                    t.CreatedAt.Month
                })
                .OrderBy(g => g.Key.Year)
                .ThenBy(g => g.Key.Month)
                .Select(g => new
                {
                    Month = $"{g.Key.Year}-{g.Key.Month:D2}",
                    Created = g.Count(),
                    Closed = g.Count(t =>
                        t.Status == TicketStatus.Closed)
                })
                .ToList()
        };
    }

    // ══════════════════════════════════════════════════════
    // UC12: BẢN ĐỒ NHIỆT
    // ══════════════════════════════════════════════════════

    public async Task<object> GetHeatmapAsync(
        string? status = null,
        string? categoryId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var allTickets = await _ticketRepo.GetAllAsync();

        // Lọc theo điều kiện
        var filtered = allTickets.AsEnumerable();

        if (!string.IsNullOrEmpty(status)
            && Enum.TryParse<TicketStatus>(status, out var s))
            filtered = filtered
                .Where(t => t.Status == s);

        if (!string.IsNullOrEmpty(categoryId))
            filtered = filtered
                .Where(t => t.CategoryId == categoryId);

        if (fromDate.HasValue)
            filtered = filtered
                .Where(t => t.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            filtered = filtered
                .Where(t => t.CreatedAt <= toDate.Value);

        // Chỉ lấy ticket có tọa độ hợp lệ
        var points = filtered
            .Where(t => t.Location != null
                && t.Location.Coordinates != null
                && t.Location.Coordinates.Length == 2
                && t.Location.Latitude != 0
                && t.Location.Longitude != 0)
            .Select(t => new
            {
                t.Id,
                t.TicketCode,
                t.Title,
                Status = t.Status.ToString(),
                Priority = t.Priority.ToString(),
                Lat = t.Location!.Latitude,
                Lng = t.Location.Longitude,
                Address = t.Location.Address,
                t.CategoryId,
                t.IsSlaBreached,
                t.CreatedAt
            })
            .ToList();

        // Thống kê cluster theo khu vực (Quận/Huyện)
        var byDistrict = allTickets
            .Where(t => !string.IsNullOrEmpty(
                t.ReporterDistrict))
            .GroupBy(t => t.ReporterDistrict!)
            .Select(g => new
            {
                District = g.Key,
                Count = g.Count()
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        return new
        {
            TotalPoints = points.Count,
            Points = points,
            ByDistrict = byDistrict
        };
    }

    // ══════════════════════════════════════════════════════
    // GIÁM SÁT SLA
    // ══════════════════════════════════════════════════════

    public async Task<object> GetSlaOverviewAsync()
    {
        var allTickets = await _ticketRepo.GetAllAsync();
        var now = DateTime.UtcNow;

        var departments = await _departmentRepo
            .GetActiveAsync();
        var deptMap = departments
            .ToDictionary(d => d.Id, d => d.Name);

        var categories = await _categoryRepo.GetAllAsync();
        var categoryMap = categories
            .ToDictionary(c => c.Id, c => c.Name);

        // Ticket đang hoạt động (chưa đóng, chưa từ chối)
        var activeStatuses = new[]
        {
            TicketStatus.New,
            TicketStatus.UnderReview,
            TicketStatus.Assigned,
            TicketStatus.InProgress,
            TicketStatus.PendingVerification
        };

        var activeTickets = allTickets
            .Where(t => activeStatuses.Contains(t.Status))
            .ToList();

        // Phân loại SLA
        var slaBreached = activeTickets
            .Where(t => t.IsSlaBreached
                || (t.SlaDeadline != null
                    && t.SlaDeadline <= now))
            .ToList();

        var slaWarning = activeTickets
            .Where(t => t.SlaDeadline != null
                && !t.IsSlaBreached
                && t.SlaDeadline > now
                && (t.SlaDeadline.Value - now).TotalHours
                    <= t.SlaHours * 0.2)
            .ToList();

        var slaOnTrack = activeTickets
            .Except(slaBreached)
            .Except(slaWarning)
            .ToList();

        // Thống kê SLA theo đơn vị
        var slaDepartment = activeTickets
            .Where(t => t.AssignedDepartmentId != null)
            .GroupBy(t => t.AssignedDepartmentId!)
            .Select(g => new
            {
                DepartmentId = g.Key,
                DepartmentName = deptMap
                    .GetValueOrDefault(g.Key, "Không xác định"),
                Total = g.Count(),
                OnTrack = g.Count(t =>
                    !t.IsSlaBreached
                    && (t.SlaDeadline == null
                        || t.SlaDeadline > now)),
                Breached = g.Count(t =>
                    t.IsSlaBreached
                    || (t.SlaDeadline != null
                        && t.SlaDeadline <= now))
            })
            .OrderByDescending(x => x.Breached)
            .ToList();

        // Helper: map ticket sang response
        object MapTicket(Ticket t) => new
        {
            t.Id,
            t.TicketCode,
            t.Title,
            Status = t.Status.ToString(),
            Priority = t.Priority.ToString(),
            CategoryName = t.CategoryId != null
                ? categoryMap
                    .GetValueOrDefault(t.CategoryId, "")
                : "",
            DepartmentName = t.AssignedDepartmentId != null
                ? deptMap
                    .GetValueOrDefault(
                        t.AssignedDepartmentId, "")
                : "",
            t.SlaHours,
            t.SlaDeadline,
            OverdueHours = t.SlaDeadline != null
                && t.SlaDeadline <= now
                ? Math.Round(
                    (now - t.SlaDeadline.Value).TotalHours, 1)
                : 0,
            RemainingHours = t.SlaDeadline != null
                && t.SlaDeadline > now
                ? Math.Round(
                    (t.SlaDeadline.Value - now).TotalHours, 1)
                : 0,
            t.CreatedAt
        };

        return new
        {
            // Tổng quan
            ActiveTickets = activeTickets.Count,
            SlaOnTrack = slaOnTrack.Count,
            SlaWarning = slaWarning.Count,
            SlaBreached = slaBreached.Count,

            // Theo đơn vị
            ByDepartment = slaDepartment,

            // Chi tiết ticket vi phạm SLA (top 20)
            BreachedTickets = slaBreached
                .OrderBy(t => t.SlaDeadline)
                .Take(20)
                .Select(MapTicket)
                .ToList(),

            // Chi tiết ticket sắp hết hạn (top 20)
            WarningTickets = slaWarning
                .OrderBy(t => t.SlaDeadline)
                .Take(20)
                .Select(MapTicket)
                .ToList()
        };
    }

    // ══════════════════════════════════════════════════════
    // UC14: XUẤT BÁO CÁO (CSV)
    // ══════════════════════════════════════════════════════

    public async Task<byte[]> ExportCsvAsync(
        string? status = null,
        string? departmentId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null)
    {
        var allTickets = await _ticketRepo.GetAllAsync();
        var categories = await _categoryRepo.GetAllAsync();
        var departments = await _departmentRepo
            .GetActiveAsync();

        var categoryMap = categories
            .ToDictionary(c => c.Id, c => c.Name);
        var deptMap = departments
            .ToDictionary(d => d.Id, d => d.Name);

        // Áp dụng bộ lọc
        var filtered = allTickets.AsEnumerable();

        if (!string.IsNullOrEmpty(status)
            && Enum.TryParse<TicketStatus>(status, out var s))
            filtered = filtered
                .Where(t => t.Status == s);

        if (!string.IsNullOrEmpty(departmentId))
            filtered = filtered
                .Where(t => t.AssignedDepartmentId
                    == departmentId);

        if (fromDate.HasValue)
            filtered = filtered
                .Where(t => t.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            filtered = filtered
                .Where(t => t.CreatedAt <= toDate.Value);

        var tickets = filtered.ToList();

        // Tạo CSV
        var lines = new List<string>
        {
            "Mã phản ánh,Tiêu đề,Trạng thái,Mức ưu tiên," +
            "Danh mục,Đơn vị xử lý," +
            "Người báo cáo,SĐT,Địa chỉ,Phường/Xã,Quận/Huyện," +
            "SLA (giờ),Hạn SLA,Vi phạm SLA," +
            "Ngày tạo,Ngày duyệt,Ngày đóng"
        };

        foreach (var t in tickets)
        {
            var categoryName = t.CategoryId != null
                ? categoryMap
                    .GetValueOrDefault(t.CategoryId, "")
                : "";
            var deptName = t.AssignedDepartmentId != null
                ? deptMap
                    .GetValueOrDefault(
                        t.AssignedDepartmentId, "")
                : "";

            var line = string.Join(",",
                Esc(t.TicketCode),
                Esc(t.Title),
                Esc(t.Status.ToString()),
                Esc(t.Priority.ToString()),
                Esc(categoryName),
                Esc(deptName),
                Esc(t.ReporterName),
                Esc(t.ReporterPhone),
                Esc(t.ReporterAddress),
                Esc(t.ReporterWard),
                Esc(t.ReporterDistrict),
                t.SlaHours.ToString(),
                t.SlaDeadline?
                    .ToString("dd/MM/yyyy HH:mm") ?? "",
                t.IsSlaBreached ? "Có" : "Không",
                t.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                t.ReviewedAt?
                    .ToString("dd/MM/yyyy HH:mm") ?? "",
                t.ClosedAt?
                    .ToString("dd/MM/yyyy HH:mm") ?? ""
            );
            lines.Add(line);
        }

        var csv = string.Join("\n", lines);

        // UTF-8 BOM để Excel đọc tiếng Việt đúng
        var bom = new byte[] { 0xEF, 0xBB, 0xBF };
        var content = System.Text.Encoding.UTF8
            .GetBytes(csv);
        var result = new byte[bom.Length + content.Length];
        bom.CopyTo(result, 0);
        content.CopyTo(result, bom.Length);

        return result;
    }

    // ── Helper ────────────────────────────────────────────
    private static string Esc(string? value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(',') || value.Contains('"')
            || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";
        return value;
    }
}