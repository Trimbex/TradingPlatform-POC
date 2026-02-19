using MediatR;
using Microsoft.AspNetCore.Mvc;
using TradingPlatform.Application.Commands;
using TradingPlatform.Application.Queries;

namespace TradingPlatform.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>Place a new order.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request, CancellationToken cancellationToken)
    {
        var command = new PlaceOrderCommand(request.UserId, request.Symbol, request.Quantity, request.Price);
        var orderId = await _mediator.Send(command, cancellationToken);
        return Ok(orderId);
    }

    /// <summary>Get order by ID.</summary>
    [HttpGet("{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var query = new GetOrderQuery(orderId);
        var order = await _mediator.Send(query, cancellationToken);
        return order is null ? NotFound() : Ok(order);
    }

    /// <summary>Get all orders for a user.</summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders([FromQuery] string userId, CancellationToken cancellationToken)
    {
        var query = new GetOrdersQuery(userId);
        var orders = await _mediator.Send(query, cancellationToken);
        return Ok(orders);
    }

    /// <summary>Cancel a pending order.</summary>
    [HttpDelete("{orderId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelOrder(Guid orderId, CancellationToken cancellationToken)
    {
        var command = new CancelOrderCommand(orderId);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}

public record PlaceOrderRequest(string UserId, string Symbol, decimal Quantity, decimal Price);
