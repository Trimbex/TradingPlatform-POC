using Microsoft.EntityFrameworkCore;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Infrastructure.Persistence.Repositories;

public class PortfolioRepository : IPortfolioRepository
{
    private readonly TradingDbContext _context;

    public PortfolioRepository(TradingDbContext context)
    {
        _context = context;
    }

    public async Task<Portfolio?> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await _context.Portfolios
            .FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }

    public async Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
    {
        await _context.Portfolios.AddAsync(portfolio, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Portfolio portfolio, CancellationToken cancellationToken = default)
    {
        _context.Portfolios.Update(portfolio);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
