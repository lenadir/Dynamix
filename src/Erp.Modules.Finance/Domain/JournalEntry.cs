namespace Erp.Modules.Finance.Domain;

/// <summary>
/// Aggregate root for double-entry journal entries.
/// </summary>
public class JournalEntry
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation id to ensure idempotent journal creation from integration events.
    /// For SalesOrderPlaced, this holds the SalesOrderId.
    /// </summary>
    public Guid? CorrelationId { get; set; }

    private readonly List<JournalEntryLine> _lines = new();
    public IReadOnlyCollection<JournalEntryLine> Lines => _lines.AsReadOnly();

    public void AddLine(Guid accountId, decimal debit, decimal credit, string description)
    {
        _lines.Add(new JournalEntryLine
        {
            Id = Guid.NewGuid(),
            JournalEntryId = Id,
            AccountId = accountId,
            Debit = debit,
            Credit = credit,
            Description = description
        });
    }
}
