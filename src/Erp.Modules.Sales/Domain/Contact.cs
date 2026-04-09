namespace Erp.Modules.Sales.Domain;

/// <summary>
/// Represents a customer / business contact within the Sales bounded context.
/// </summary>
public class Contact
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}
