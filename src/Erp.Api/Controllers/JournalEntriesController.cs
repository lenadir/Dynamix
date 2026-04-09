using Erp.Modules.Finance.Queries;
using Erp.Shared.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Erp.Api.Controllers;

[ApiController]
[Route("api/finance/journal-entries")]
public class JournalEntriesController : ControllerBase
{
    private readonly IMediator _mediator;
    public JournalEntriesController(IMediator mediator) => _mediator = mediator;

    /// <summary>GET /api/finance/journal-entries – List recent journal entries.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<JournalEntryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var result = await _mediator.Send(new GetJournalEntriesQuery(), ct);
        return Ok(result);
    }
}
