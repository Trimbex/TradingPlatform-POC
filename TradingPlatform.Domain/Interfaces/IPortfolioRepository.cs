using TradingPlatform.Domain.Entities;

namespace TradingPlatform.Domain.Interfaces;

public interface IPortfolioRepository
{
    Task<Portfolio?> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
    Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default);
    Task UpdateAsync(Portfolio portfolio, CancellationToken cancellationToken = default);
}
