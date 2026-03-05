using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradingPlatform.Application.Queries;

namespace TradingPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TransactionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get all transactions for a user.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions([FromQuery] string userId, CancellationToken cancellationToken)
    {
        var query = new GetTransactionsQuery(userId);
        var transactions = await _mediator.Send(query, cancellationToken);
        return Ok(transactions);
    }
}

