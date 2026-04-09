using Erp.Modules.Finance.Domain;
using Erp.Platform.Tenancy;
using Erp.Shared.Events;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Erp.Modules.Finance.Consumers;

/// <summary>
/// MassTransit consumer that reacts to <see cref="SalesOrderPlaced"/> events
/// published by the Sales module and creates a corresponding journal entry
/// (Debit Accounts Receivable, Credit Revenue).
/// Idempotent: checks CorrelationId to avoid duplicate entries.
/// </summary>
public class SalesOrderPlacedConsumer : IConsumer<SalesOrderPlaced>
{
    private readonly DbContext _db;
    private readonly ITenantProvider _tenantProvider;
    private readonly ILogger<SalesOrderPlacedConsumer> _logger;

    // Well-known seed account ids (match the seed data in ErpDbContext)
    public static readonly Guid AccountsReceivableId = new("00000000-0000-0000-0000-000000001100");
    public static readonly Guid RevenueId = new("00000000-0000-0000-0000-000000004000");

    public SalesOrderPlacedConsumer(DbContext db, ITenantProvider tenantProvider, ILogger<SalesOrderPlacedConsumer> logger)
    {
        _db = db;
        _tenantProvider = tenantProvider;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<SalesOrderPlaced> context)
    {
        var evt = context.Message;

        // Set tenant context from the event so the query filter works outside HTTP requests
        _tenantProvider.CurrentTenantId = evt.TenantId;

        // Idempotency: skip if a journal entry already exists for this order
        var exists = await _db.Set<JournalEntry>()
            .AnyAsync(j => j.CorrelationId == evt.SalesOrderId, context.CancellationToken);

        if (exists)
        {
            _logger.LogInformation("Journal entry for SalesOrder {OrderId} already exists, skipping.", evt.SalesOrderId);
            return;
        }

        var entry = new JournalEntry
        {
            TenantId = evt.TenantId,
            CorrelationId = evt.SalesOrderId,
            Date = evt.OccurredOn
        };

        entry.AddLine(AccountsReceivableId, evt.Total, 0, $"AR for {evt.OrderNumber}");
        entry.AddLine(RevenueId, 0, evt.Total, $"Revenue for {evt.OrderNumber}");

        _db.Set<JournalEntry>().Add(entry);
        await _db.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Created journal entry {EntryId} for SalesOrder {OrderId}.", entry.Id, evt.SalesOrderId);
    }
}
