using MediatR;

namespace TradingPlatform.Application.Commands;

public record WithdrawFundsCommand(string UserId, decimal Amount) : IRequest<Unit>;
