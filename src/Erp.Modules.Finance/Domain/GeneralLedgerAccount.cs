namespace Erp.Modules.Finance.Domain;

/// <summary>
/// Represents an account in the Chart of Accounts (e.g., 1100 - Accounts Receivable).
/// </summary>
public class GeneralLedgerAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string AccountNumber { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // Asset, Liability, Revenue, Expense, Equity
}
