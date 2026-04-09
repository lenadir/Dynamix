namespace Erp.Platform.Entities;

/// <summary>
/// Represents a tenant (company / legal entity) in the multi-tenant ERP.
/// Every aggregate root references TenantId for data isolation.
/// </summary>
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public string DefaultCurrency { get; set; } = "USD";
}
