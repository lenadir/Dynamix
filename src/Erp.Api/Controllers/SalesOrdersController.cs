using Erp.Modules.Sales.Commands;
using Erp.Modules.Sales.Queries;
using Erp.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers;

[ApiController]
[Route("api/sales/orders")]
public class SalesOrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    public SalesOrdersController(IMediator mediator) => _mediator = mediator;

    /// <summary>GET /api/sales/orders – List all orders for the current tenant.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<SalesOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSalesOrdersQuery(), ct);
        return Ok(result);
    }

    /// <summary>POST /api/sales/orders – Create a new sales order.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(SalesOrderDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderDto dto, CancellationToken ct)
    {
        var result = await _mediator.Send(new CreateSalesOrderCommand(dto), ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>GET /api/sales/orders/{id} – Get an order by id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(SalesOrderDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await _mediator.Send(new GetSalesOrderByIdQuery(id), ct);
        return result is null ? NotFound() : Ok(result);
    }
}
