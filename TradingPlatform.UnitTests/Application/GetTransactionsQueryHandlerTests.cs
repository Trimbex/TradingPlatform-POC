using FluentAssertions;
using TradingPlatform.Application.DTOs;
using TradingPlatform.Application.Queries;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Enums;
using TradingPlatform.Domain.Interfaces;
using Xunit;

namespace TradingPlatform.UnitTests.Application;

public class GetTransactionsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserHasTransactions_ReturnsAllTransactions()
    {
        var tx1 = Transaction.Create("user-1", null, TransactionType.Deposit, 100m, TransactionStatus.Completed);
        var tx2 = Transaction.Create("user-1", Guid.NewGuid(), TransactionType.OrderPayment, 250m, TransactionStatus.Completed);
        var repository = new StubTransactionRepository([tx1, tx2]);
        var handler = new GetTransactionsQueryHandler(repository);

        var result = await handler.Handle(new GetTransactionsQuery("user-1"), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(t => t.Amount == 100m && t.Type == nameof(TransactionType.Deposit));
        result.Should().Contain(t => t.Amount == 250m && t.Type == nameof(TransactionType.OrderPayment));
    }

    [Fact]
    public async Task Handle_WhenUserHasNoTransactions_ReturnsEmptyList()
    {
        var repository = new StubTransactionRepository([]);
        var handler = new GetTransactionsQueryHandler(repository);

        var result = await handler.Handle(new GetTransactionsQuery("user-1"), CancellationToken.None);

        result.Should().BeEmpty();
    }

    private class StubTransactionRepository : ITransactionRepository
    {
        private readonly IReadOnlyList<Transaction> _transactions;

        public StubTransactionRepository(IReadOnlyList<Transaction> transactions)
        {
            _transactions = transactions;
        }

        public Task AddAsync(Transaction transaction, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task<IEnumerable<Transaction>> GetByUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            var result = _transactions.Where(t => t.UserId == userId);
            return Task.FromResult(result);
        }
    }
}

