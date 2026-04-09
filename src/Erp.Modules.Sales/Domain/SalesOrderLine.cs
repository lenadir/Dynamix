namespace Erp.Modules.Sales.Domain;

public class SalesOrderLine
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SalesOrderId { get; set; }
    public Guid ProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
}
