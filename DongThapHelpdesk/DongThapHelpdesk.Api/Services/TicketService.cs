using DongThapHelpdesk.Api.Repositories;
using DongThapHelpdesk.Api.Models;

namespace DongThapHelpdesk.Api.Services;

public class TicketService
{
    private readonly TicketRepository _repo;

    public TicketService(TicketRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Ticket>> GetAllAsync()
        => await _repo.GetAllAsync();

    public async Task<Ticket?> GetByIdAsync(string id)
        => await _repo.GetByIdAsync(id);

    public async Task<Ticket?> GetByCodeAsync(string code)
        => await _repo.GetByCodeAsync(code);

    public async Task<List<Ticket>> GetByDepartmentAsync(
        string departmentId)
        => await _repo.GetByDepartmentAsync(departmentId);

    public async Task<List<Ticket>> GetByCitizenIdAsync(
        string citizenId)
        => await _repo.GetByCitizenIdAsync(citizenId);
    // Lấy ticket của Citizen đang đăng nhập

    public async Task<bool> IsFirstTicketOfMonthAsync(
        string citizenId)
    {
        var now = DateTime.UtcNow;
        var count = await _repo.CountByCitizenInMonthAsync(
            citizenId, now.Month, now.Year);
        return count == 0;
    }
    // Kiểm tra đây có phải báo cáo đầu tiên trong tháng không
    // Dùng để cộng điểm thưởng FirstTicketOfMonth
}