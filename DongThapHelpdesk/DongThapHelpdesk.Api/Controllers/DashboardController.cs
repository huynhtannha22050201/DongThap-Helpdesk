using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DongThapHelpdesk.Api.Constants;
using DongThapHelpdesk.Api.Services;

namespace DongThapHelpdesk.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = Roles.AdminAndManager)]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _service;

    public DashboardController(DashboardService service)
    {
        _service = service;
    }

    // ══════════════════════════════════════════════════════
    // UC13: THỐNG KÊ TỔNG QUAN
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Thống kê tổng quan toàn hệ thống
    /// Bao gồm: tổng ticket, theo trạng thái, danh mục,
    /// đơn vị, mức ưu tiên, xu hướng 30 ngày + 12 tháng,
    /// thời gian xử lý trung bình, tỷ lệ SLA
    /// </summary>
    [HttpGet("stats")]
    public async Task<IActionResult> GetStats()
        => Ok(await _service.GetStatsAsync());

    // ══════════════════════════════════════════════════════
    // UC12: BẢN ĐỒ NHIỆT
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Trả về tọa độ các ticket để hiển thị trên bản đồ
    /// Hỗ trợ lọc theo trạng thái, danh mục, khoảng thời gian
    /// </summary>
    /// <param name="status">Lọc theo trạng thái (New, InProgress...)</param>
    /// <param name="categoryId">Lọc theo danh mục sự cố</param>
    /// <param name="fromDate">Từ ngày (yyyy-MM-dd)</param>
    /// <param name="toDate">Đến ngày (yyyy-MM-dd)</param>
    [HttpGet("heatmap")]
    public async Task<IActionResult> GetHeatmap(
        [FromQuery] string? status = null,
        [FromQuery] string? categoryId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
        => Ok(await _service.GetHeatmapAsync(
            status, categoryId, fromDate, toDate));

    // ══════════════════════════════════════════════════════
    // GIÁM SÁT SLA
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Giám sát tình hình SLA toàn hệ thống
    /// Phân loại: đúng hạn, sắp hết hạn, vi phạm
    /// Chi tiết theo đơn vị xử lý
    /// </summary>
    [HttpGet("sla")]
    public async Task<IActionResult> GetSlaOverview()
        => Ok(await _service.GetSlaOverviewAsync());

    // ══════════════════════════════════════════════════════
    // UC14: XUẤT BÁO CÁO
    // ══════════════════════════════════════════════════════

    /// <summary>
    /// Xuất báo cáo dạng CSV
    /// Hỗ trợ lọc theo trạng thái, đơn vị, khoảng thời gian
    /// File CSV có UTF-8 BOM để Excel đọc tiếng Việt đúng
    /// </summary>
    /// <param name="status">Lọc theo trạng thái</param>
    /// <param name="departmentId">Lọc theo đơn vị xử lý</param>
    /// <param name="fromDate">Từ ngày (yyyy-MM-dd)</param>
    /// <param name="toDate">Đến ngày (yyyy-MM-dd)</param>
    [HttpGet("export")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] string? status = null,
        [FromQuery] string? departmentId = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        var csvBytes = await _service.ExportCsvAsync(
            status, departmentId, fromDate, toDate);

        var fileName =
            $"bao-cao-phan-anh-{DateTime.UtcNow:yyyyMMdd-HHmmss}.csv";

        return File(
            csvBytes,
            "text/csv; charset=utf-8",
            fileName);
    }
}