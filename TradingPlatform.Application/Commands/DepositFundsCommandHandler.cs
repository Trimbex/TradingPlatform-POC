using MediatR;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Commands;

public class DepositFundsCommandHandler : IRequestHandler<DepositFundsCommand, Unit>
{
    private readonly IPortfolioRepository _portfolioRepository;

    public DepositFundsCommandHandler(IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<Unit> Handle(DepositFundsCommand request, CancellationToken cancellationToken)
    {
        var portfolio = await _portfolioRepository.GetByUserAsync(request.UserId, cancellationToken);

        if (portfolio is null)
        {
            portfolio = Portfolio.Create(request.UserId);
            await _portfolioRepository.AddAsync(portfolio, cancellationToken);
        }

        portfolio.AddFunds(request.Amount);
        await _portfolioRepository.UpdateAsync(portfolio, cancellationToken);

        return Unit.Value;
    }
}
