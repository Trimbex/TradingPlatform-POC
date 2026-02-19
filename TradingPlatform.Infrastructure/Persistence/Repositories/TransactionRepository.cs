using Microsoft.EntityFrameworkCore;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Infrastructure.Persistence.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly TradingDbContext _context;

    public TransactionRepository(TradingDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default)
    {
        await _context.Transactions.AddAsync(transaction, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var orderIds = await _context.Orders
            .Where(o => o.UserId == userId)
            .Select(o => o.Id)
            .ToListAsync(cancellationToken);

        return await _context.Transactions
            .Where(t => t.OrderId != null && orderIds.Contains(t.OrderId.Value))
            .OrderByDescending(t => t.Timestamp)
            .ToListAsync(cancellationToken);
    }
}
