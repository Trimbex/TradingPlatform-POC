using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradingPlatform.Application.Commands;
using TradingPlatform.Application.Queries;

namespace TradingPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PortfolioController : ControllerBase
{
    private readonly IMediator _mediator;

    public PortfolioController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Get portfolio by user ID.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPortfolio([FromQuery] string userId, CancellationToken cancellationToken)
    {
        var query = new GetPortfolioQuery(userId);
        var portfolio = await _mediator.Send(query, cancellationToken);
        return portfolio is null ? NotFound() : Ok(portfolio);
    }

    /// <summary>Deposit funds into portfolio. Creates portfolio if it doesn't exist.</summary>
    [HttpPost("deposit")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Deposit([FromBody] DepositRequest request, CancellationToken cancellationToken)
    {
        var command = new DepositFundsCommand(request.UserId, request.Amount);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }

    /// <summary>Withdraw funds from portfolio.</summary>
    [HttpPost("withdraw")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Withdraw([FromBody] WithdrawRequest request, CancellationToken cancellationToken)
    {
        var command = new WithdrawFundsCommand(request.UserId, request.Amount);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public record DepositRequest(string UserId, decimal Amount);
public record WithdrawRequest(string UserId, decimal Amount);
