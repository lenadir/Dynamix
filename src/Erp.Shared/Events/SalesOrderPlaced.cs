namespace Erp.Shared.Events;

/// <summary>
/// Integration event published via MassTransit when a sales order is persisted.
/// Both Sales and Finance modules reference this contract through Erp.Shared.
/// </summary>
public record SalesOrderPlaced(
    Guid SalesOrderId,
    Guid TenantId,
    string OrderNumber,
    decimal Total,
    DateTime OccurredOn);
