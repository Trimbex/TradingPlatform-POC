using TradingPlatform.Domain.Entities;

namespace TradingPlatform.Domain.Interfaces;

public interface ITransactionRepository
{
    Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default);
    Task<IEnumerable<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default);
}
