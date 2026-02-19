using MediatR;

namespace TradingPlatform.Application.Commands;

public record CancelOrderCommand(Guid OrderId) : IRequest<Unit>;
