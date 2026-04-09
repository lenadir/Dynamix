using Erp.Modules.Sales.Domain;
using Erp.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Erp.Modules.Sales.Queries;

// ── Query ────────────────────────────────────────────────────────────────────
public record GetSalesOrderByIdQuery(Guid Id) : IRequest<SalesOrderDto?>;

// ── Handler ──────────────────────────────────────────────────────────────────
public class GetSalesOrderByIdHandler : IRequestHandler<GetSalesOrderByIdQuery, SalesOrderDto?>
{
    private readonly DbContext _db;

    public GetSalesOrderByIdHandler(DbContext db) => _db = db;

    public async Task<SalesOrderDto?> Handle(GetSalesOrderByIdQuery request, CancellationToken ct)
    {
        var order = await _db.Set<SalesOrder>()
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == request.Id, ct);

        if (order is null) return null;

        return new SalesOrderDto(
            order.Id, order.TenantId, order.OrderNumber, order.CustomerId,
            order.Date, order.Total,
            order.Lines.Select(l => new SalesOrderLineDto(
                l.ProductId, l.Quantity, l.UnitPrice, l.LineTotal)).ToList());
    }
}
