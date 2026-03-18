using DongThapHelpdesk.Api.Repositories;

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
}