using TradingPlatform.Domain.Interfaces;

namespace TradingPlatform.Infrastructure.Persistence;

/// <summary>
/// No-op implementation for environments (e.g. InMemory tests) where the database
/// does not support transactions. All operations succeed without starting a real transaction.
/// </summary>
public class NoOpUnitOfWork : IUnitOfWork
{
    public Task BeginTransactionAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
    public Task RollbackAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
}
