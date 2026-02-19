using MediatR;

namespace TradingPlatform.Application.Commands;

public record DepositFundsCommand(string UserId, decimal Amount) : IRequest<Unit>;
