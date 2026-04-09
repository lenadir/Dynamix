using Erp.Modules.Finance.Domain;
using Erp.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Erp.Modules.Finance.Queries;

public record GetJournalEntriesQuery : IRequest<List<JournalEntryDto>>;

public class GetJournalEntriesHandler : IRequestHandler<GetJournalEntriesQuery, List<JournalEntryDto>>
{
    private readonly DbContext _db;
    public GetJournalEntriesHandler(DbContext db) => _db = db;

    public async Task<List<JournalEntryDto>> Handle(GetJournalEntriesQuery request, CancellationToken ct)
    {
        var entries = await _db.Set<JournalEntry>()
            .Include(e => e.Lines)
            .OrderByDescending(e => e.Date)
            .Take(100)
            .ToListAsync(ct);

        return entries.Select(e => new JournalEntryDto(
            e.Id, e.TenantId, e.Date,
            e.Lines.Select(l => new JournalEntryLineDto(
                l.AccountId, l.Debit, l.Credit, l.Description)).ToList()
        )).ToList();
    }
}
