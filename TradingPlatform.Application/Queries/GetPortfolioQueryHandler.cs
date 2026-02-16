using MediatR;
using TradingPlatform.Application.DTOs;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Application.Queries;

public class GetPortfolioQueryHandler : IRequestHandler<GetPortfolioQuery, PortfolioDto?>
{
    private readonly IPortfolioRepository _portfolioRepository;

    public GetPortfolioQueryHandler(IPortfolioRepository portfolioRepository)
    {
        _portfolioRepository = portfolioRepository;
    }

    public async Task<PortfolioDto?> Handle(GetPortfolioQuery request, CancellationToken cancellationToken)
    {
        var portfolio = await _portfolioRepository.GetByUserAsync(request.UserId, cancellationToken);

        if (portfolio is null)
            return null;

        var holdings = portfolio.Holdings
            .Select(h => new HoldingDto(h.Symbol, h.Quantity, h.AveragePrice))
            .ToList();

        return new PortfolioDto(
            portfolio.Id,
            portfolio.UserId,
            portfolio.CashBalance,
            holdings
        );
    }
}
