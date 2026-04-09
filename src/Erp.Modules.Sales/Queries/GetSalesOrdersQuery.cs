using Erp.Modules.Sales.Domain;
using Erp.Shared.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Erp.Modules.Sales.Queries;

public record GetSalesOrdersQuery : IRequest<List<SalesOrderDto>>;

public class GetSalesOrdersHandler : IRequestHandler<GetSalesOrdersQuery, List<SalesOrderDto>>
{
    private readonly DbContext _db;
    public GetSalesOrdersHandler(DbContext db) => _db = db;

    public async Task<List<SalesOrderDto>> Handle(GetSalesOrdersQuery request, CancellationToken ct)
    {
        var orders = await _db.Set<SalesOrder>()
            .Include(o => o.Lines)
            .OrderByDescending(o => o.Date)
            .Take(100)
            .ToListAsync(ct);

        return orders.Select(o => new SalesOrderDto(
            o.Id, o.TenantId, o.OrderNumber, o.CustomerId,
            o.Date, o.Total,
            o.Lines.Select(l => new SalesOrderLineDto(
                l.ProductId, l.Quantity, l.UnitPrice, l.LineTotal)).ToList()
        )).ToList();
    }
}
