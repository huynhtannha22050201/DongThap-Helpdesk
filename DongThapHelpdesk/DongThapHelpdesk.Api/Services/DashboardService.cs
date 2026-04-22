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

    public async Task<object> GetStatsAsync(string period = "month")
{
    var allTickets = await _ticketRepo.GetAllAsync();
    var now = DateTime.UtcNow;

    // ══════════════════════════════════════════════════════
    // TÍNH KHOẢNG THỜI GIAN HIỆN TẠI + KỲ TRƯỚC
    // ══════════════════════════════════════════════════════

    DateTime currentStart, previousStart, previousEnd;

    switch (period.ToLower())
    {
        case "week":
            // Đầu tuần hiện tại (Monday)
            var dayOfWeek = ((int)now.DayOfWeek + 6) % 7;
            currentStart = new DateTime(
                now.Year, now.Month, now.Day, 0, 0, 0, DateTimeKind.Utc)
                .AddDays(-dayOfWeek);
            previousStart = currentStart.AddDays(-7);
            previousEnd = currentStart;
            break;

        case "quarter":
            var currentQuarter = (now.Month - 1) / 3;
            currentStart = new DateTime(
                now.Year, currentQuarter * 3 + 1, 1, 0, 0, 0, DateTimeKind.Utc);
            previousStart = currentStart.AddMonths(-3);
            previousEnd = currentStart;
            break;

        default: // "month"
            currentStart = new DateTime(
                now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            previousStart = currentStart.AddMonths(-1);
            previousEnd = currentStart;
            break;
    }

    // Ticket kỳ hiện tại vs kỳ trước
    var currentTickets = allTickets
        .Where(t => t.CreatedAt >= currentStart).ToList();
    var previousTickets = allTickets
        .Where(t => t.CreatedAt >= previousStart
                 && t.CreatedAt < previousEnd).ToList();

    // ══════════════════════════════════════════════════════
    // TÍNH TREND (% THAY ĐỔI SO VỚI KỲ TRƯỚC)
    // ══════════════════════════════════════════════════════

    int currentNew = currentTickets.Count;
    int previousNew = previousTickets.Count;
    double newTrend = previousNew > 0
        ? Math.Round(
            (double)(currentNew - previousNew) / previousNew * 100, 1)
        : 0;

    int currentClosed = currentTickets
        .Count(t => t.Status == TicketStatus.Closed);
    int previousClosed = previousTickets
        .Count(t => t.Status == TicketStatus.Closed);
    double closedTrend = previousClosed > 0
        ? Math.Round(
            (double)(currentClosed - previousClosed) / previousClosed * 100, 1)
        : 0;

    int currentBreached = currentTickets
        .Count(t => t.IsSlaBreached);
    int previousBreached = previousTickets
        .Count(t => t.IsSlaBreached);
    double breachedTrend = previousBreached > 0
        ? Math.Round(
            (double)(currentBreached - previousBreached) / previousBreached * 100, 1)
        : 0;

    var currentInProgress = currentTickets.Count(t =>
        t.Status == TicketStatus.Assigned
        || t.Status == TicketStatus.InProgress);
    var previousInProgress = previousTickets.Count(t =>
        t.Status == TicketStatus.Assigned
        || t.Status == TicketStatus.InProgress);
    double inProgressTrend = previousInProgress > 0
        ? Math.Round(
            (double)(currentInProgress - previousInProgress)
            / previousInProgress * 100, 1)
        : 0;

    // "SLA Compliance" theo kỳ
    var currentClosedTickets = currentTickets
        .Where(t => t.Status == TicketStatus.Closed
            && t.ClosedAt != null).ToList();
    var currentSlaRate = currentClosedTickets.Count > 0
        ? Math.Round(
            (double)currentClosedTickets.Count(t => !t.IsSlaBreached)
            / currentClosedTickets.Count * 100, 1)
        : 0;

    var previousClosedTickets = previousTickets
        .Where(t => t.Status == TicketStatus.Closed
            && t.ClosedAt != null).ToList();
    var previousSlaRate = previousClosedTickets.Count > 0
        ? Math.Round(
            (double)previousClosedTickets.Count(t => !t.IsSlaBreached)
            / previousClosedTickets.Count * 100, 1)
        : 0;

    double slaTrend = Math.Round(currentSlaRate - previousSlaRate, 1);

    // ══════════════════════════════════════════════════════
    // THỐNG KÊ THEO TRẠNG THÁI
    // ══════════════════════════════════════════════════════

    var byStatus = allTickets
    .GroupBy(t => t.Status)
    .ToDictionary(
        g => g.Key.ToString(),
        g => g.Count());

    // ══════════════════════════════════════════════════════
    // THỐNG KÊ THEO DANH MỤC
    // ══════════════════════════════════════════════════════

    var categories = await _categoryRepo.GetAllAsync();
    var categoryMap = categories
        .ToDictionary(c => c.Id, c => c.Name);

    var byCategory = currentTickets
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

        // ══════════════════════════════════════════════════════
        // THỐNG KÊ THEO ĐƠN VỊ XỬ LÝ
        // ══════════════════════════════════════════════════════

        var departments = await _departmentRepo.GetActiveAsync();
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

    // ══════════════════════════════════════════════════════
    // THỐNG KÊ THEO MỨC ƯU TIÊN
    // ══════════════════════════════════════════════════════

    var byPriority = allTickets
        .GroupBy(t => t.Priority)
        .ToDictionary(
            g => g.Key.ToString(),
            g => g.Count());

    // ══════════════════════════════════════════════════════
    // THỜI GIAN XỬ LÝ TRUNG BÌNH + SLA COMPLIANCE
    // ══════════════════════════════════════════════════════

    var closedTickets = allTickets
        .Where(t => t.Status == TicketStatus.Closed
            && t.ClosedAt != null)
        .ToList();

    var avgResolutionHours = closedTickets.Any()
        ? Math.Round(closedTickets
            .Average(t =>
                (t.ClosedAt!.Value - t.CreatedAt).TotalHours), 1)
        : 0;

    // ══════════════════════════════════════════════════════
    // TỔNG SỐ NGƯỜI DÂN
    // ══════════════════════════════════════════════════════

    var citizenFilter = Builders<AppUser>.Filter
        .Eq(u => u.Role, UserRole.Citizen);
    var citizens = await _userRepo
        .GetByFilterAsync(citizenFilter);

    // ══════════════════════════════════════════════════════
    // TRẢ VỀ RESPONSE
    // ══════════════════════════════════════════════════════

    return new
    {
        // ── Tổng quan toàn hệ thống ──────────────────
        TotalTickets = allTickets.Count,
        TotalClosed = allTickets
            .Count(t => t.Status == TicketStatus.Closed),
        TotalRejected = allTickets
            .Count(t => t.Status == TicketStatus.Rejected),
        TotalSlaBreached = allTickets
            .Count(t => t.IsSlaBreached),
        TotalCitizens = citizens.Count,
        // "Đang xử lý" = Assigned + InProgress (đã giao cho Assignee)
        TotalInProgress = allTickets.Count(t =>
            t.Status == TicketStatus.Assigned
            || t.Status == TicketStatus.InProgress),

        // ── Theo kỳ hiện tại (period) ────────────────
        TotalThisPeriod = currentNew,
        TotalPreviousPeriod = previousNew,
        ClosedThisPeriod = currentClosed,
        BreachedThisPeriod = currentBreached,


        // ── KPI THEO KỲ (period) ─────────────────────────
        PeriodNew = currentNew,
        PreviousNew = previousNew,
        NewTrend = newTrend,

        PeriodInProgress = currentInProgress,
        PreviousInProgress = previousInProgress,
        InProgressTrend = inProgressTrend,

        PeriodClosed = currentClosed,
        PreviousClosed = previousClosed,
        ClosedTrend = closedTrend,

        PeriodBreached = currentBreached,
        PreviousBreached = previousBreached,
        BreachedTrend = breachedTrend,

        PeriodSlaRate = currentSlaRate,
        PreviousSlaRate = previousSlaRate,
        SlaTrend = slaTrend,

        // ── Tỷ lệ ───────────────────────────────────
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

        // ── Chi tiết ─────────────────────────────────
        ByStatus = byStatus,
        ByCategory = byCategory,
        ByDepartment = byDepartment,
        ByPriority = byPriority,

        // ── Xu hướng 30 ngày ─────────────────────────
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

        // ── Xu hướng 12 tháng ────────────────────────
        ChartTrend = BuildChartTrend(allTickets, now, period),
    };
}

    /// <summary>
    /// Tạo dữ liệu biểu đồ cột theo period:
    /// - week  → 7 ngày (Thứ 2 → CN)
    /// - month → 12 tháng trong năm (T01 → T12)  
    /// - quarter → 4 quý trong năm (Q1 → Q4)
    /// "Đã xử lý" = Closed + PendingVerification
    /// </summary>
    private static List<object> BuildChartTrend(
        List<Ticket> allTickets, DateTime now, string period)
    {
        var processedStatuses = new[]
        {
        TicketStatus.Closed,
        TicketStatus.PendingVerification
    };

        switch (period.ToLower())
        {
            case "week":
                {
                    // 7 ngày của tuần hiện tại (Monday → Sunday)
                    var dayOfWeek = ((int)now.DayOfWeek + 6) % 7;
                    var monday = now.Date.AddDays(-dayOfWeek);
                    var dayLabels = new[]
                    { "Thứ 2", "Thứ 3", "Thứ 4", "Thứ 5",
              "Thứ 6", "Thứ 7", "CN" };

                    return Enumerable.Range(0, 7).Select(i =>
                    {
                        var date = monday.AddDays(i);
                        var dayTickets = allTickets
                            .Where(t => t.CreatedAt.Date == date)
                            .ToList();

                        return (object)new
                        {
                            Label = dayLabels[i],
                            Created = dayTickets.Count,
                            Processed = dayTickets
                                .Count(t => processedStatuses
                                    .Contains(t.Status))
                        };
                    }).ToList();
                }

            case "quarter":
                {
                    // 4 quý trong năm hiện tại
                    return Enumerable.Range(1, 4).Select(q =>
                    {
                        var qStart = new DateTime(
                            now.Year, (q - 1) * 3 + 1, 1,
                            0, 0, 0, DateTimeKind.Utc);
                        var qEnd = qStart.AddMonths(3);

                        var qTickets = allTickets
                            .Where(t => t.CreatedAt >= qStart
                                     && t.CreatedAt < qEnd)
                            .ToList();

                        return (object)new
                        {
                            Label = $"Q{q}",
                            Created = qTickets.Count,
                            Processed = qTickets
                                .Count(t => processedStatuses
                                    .Contains(t.Status))
                        };
                    }).ToList();
                }

            default: // "month" → 12 tháng trong năm
                {
                    return Enumerable.Range(1, 12).Select(m =>
                    {
                        var mStart = new DateTime(
                            now.Year, m, 1,
                            0, 0, 0, DateTimeKind.Utc);
                        var mEnd = mStart.AddMonths(1);

                        var mTickets = allTickets
                            .Where(t => t.CreatedAt >= mStart
                                     && t.CreatedAt < mEnd)
                            .ToList();

                        return (object)new
                        {
                            Label = $"T{m:D2}",  // T01, T02...T12
                            Created = mTickets.Count,
                            Processed = mTickets
                                .Count(t => processedStatuses
                                    .Contains(t.Status))
                        };
                    }).ToList();
                }
        }
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