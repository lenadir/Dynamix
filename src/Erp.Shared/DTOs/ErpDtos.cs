namespace Erp.Shared.DTOs;

public record CreateSalesOrderDto(
    Guid CustomerId,
    List<CreateSalesOrderLineDto> Lines);

public record CreateSalesOrderLineDto(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice);

public record SalesOrderDto(
    Guid Id,
    Guid TenantId,
    string OrderNumber,
    Guid CustomerId,
    DateTime Date,
    decimal Total,
    List<SalesOrderLineDto> Lines);

public record SalesOrderLineDto(
    Guid ProductId,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal);

public record JournalEntryDto(
    Guid Id,
    Guid TenantId,
    DateTime Date,
    List<JournalEntryLineDto> Lines);

public record JournalEntryLineDto(
    Guid AccountId,
    decimal Debit,
    decimal Credit,
    string Description);
