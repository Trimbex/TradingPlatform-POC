using MediatR;

namespace TradingPlatform.Application.Commands;

public record PlaceOrderCommand(
    string UserId,
    string Symbol,
    decimal Quantity,
    decimal Price
) : IRequest<Guid>;
