namespace Erp.Modules.Sales.Domain;

/// <summary>
/// Aggregate root for a sales order. TenantId is used by the global
/// query filter in ErpDbContext for multi-tenant data isolation.
/// </summary>
public class SalesOrder
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid TenantId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid CustomerId { get; set; }
    public DateTime Date { get; set; } = DateTime.UtcNow;
    public decimal Total { get; private set; }

    private readonly List<SalesOrderLine> _lines = new();
    public IReadOnlyCollection<SalesOrderLine> Lines => _lines.AsReadOnly();

    // EF Core needs a parameterless ctor
    private SalesOrder() { }

    public SalesOrder(Guid tenantId, Guid customerId, string orderNumber)
    {
        TenantId = tenantId;
        CustomerId = customerId;
        OrderNumber = orderNumber;
    }

    public void AddLine(Guid productId, decimal quantity, decimal unitPrice)
    {
        var line = new SalesOrderLine
        {
            Id = Guid.NewGuid(),
            SalesOrderId = Id,
            ProductId = productId,
            Quantity = quantity,
            UnitPrice = unitPrice,
            LineTotal = quantity * unitPrice
        };
        _lines.Add(line);
        RecalculateTotal();
    }

    private void RecalculateTotal() => Total = _lines.Sum(l => l.LineTotal);
}
