using MediatR;
using TradingPlatform.Application.Exceptions;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Commands;

public class WithdrawFundsCommandHandler : IRequestHandler<WithdrawFundsCommand, Unit>
{
    private readonly IPortfolioRepository _portfolioRepository;

    public WithdrawFundsCommandHandler(IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<Unit> Handle(WithdrawFundsCommand request, CancellationToken cancellationToken)
    {
        var portfolio = await _portfolioRepository.GetByUserAsync(request.UserId, cancellationToken)
            ?? throw new NotFoundException($"Portfolio for user {request.UserId} not found.");

        portfolio.Withdraw(request.Amount);
        await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);

        return Unit.Value;
    }
}
