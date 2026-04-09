using Erp.Modules.Sales.Domain;
using Erp.Platform.Tenancy;
using Erp.Shared.DTOs;
using Erp.Shared.Events;
using FluentValidation;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Erp.Modules.Sales.Commands;

// ── Command ──────────────────────────────────────────────────────────────────
public record CreateSalesOrderCommand(CreateSalesOrderDto Dto) : IRequest<SalesOrderDto>;

// ── Handler ──────────────────────────────────────────────────────────────────
public class CreateSalesOrderHandler : IRequestHandler<CreateSalesOrderCommand, SalesOrderDto>
{
    private readonly DbContext _db;
    private readonly ITenantProvider _tenant;
    private readonly IPublishEndpoint _bus;
    private readonly IValidator<CreateSalesOrderDto> _validator;

    public CreateSalesOrderHandler(
        DbContext db,
        ITenantProvider tenant,
        IPublishEndpoint bus,
        IValidator<CreateSalesOrderDto> validator)
    {
        _db = db;
        _tenant = tenant;
        _bus = bus;
        _validator = validator;
    }

    public async Task<SalesOrderDto> Handle(CreateSalesOrderCommand request, CancellationToken ct)
    {
        var result = await _validator.ValidateAsync(request.Dto, ct);
        if (!result.IsValid)
            throw new ValidationException(result.Errors);

        // Generate a simple sequential order number (production: use a sequence / saga)
        var orderNumber = $"SO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpperInvariant()}";

        var order = new SalesOrder(_tenant.CurrentTenantId, request.Dto.CustomerId, orderNumber);

        foreach (var line in request.Dto.Lines)
            order.AddLine(line.ProductId, line.Quantity, line.UnitPrice);

        _db.Set<SalesOrder>().Add(order);
        await _db.SaveChangesAsync(ct);

        // Publish integration event for cross-module communication
        await _bus.Publish(new SalesOrderPlaced(
            order.Id,
            order.TenantId,
            order.OrderNumber,
            order.Total,
            DateTime.UtcNow), ct);

        return new SalesOrderDto(
            order.Id, order.TenantId, order.OrderNumber, order.CustomerId,
            order.Date, order.Total,
            order.Lines.Select(l => new SalesOrderLineDto(
                l.ProductId, l.Quantity, l.UnitPrice, l.LineTotal)).ToList());
    }
}
