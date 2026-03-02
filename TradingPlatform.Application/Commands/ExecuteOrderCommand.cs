using MediatR;

namespace TradingPlatform.Application.Commands;

public record ExecuteOrderCommand(Guid OrderId) : IRequest<Unit>;
