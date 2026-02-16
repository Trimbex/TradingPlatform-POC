using FluentAssertions;
using TradingPlatform.Application.DTOs;
using TradingPlatform.Application.Queries;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Interfaces;
using Xunit;

namespace TradingPlatform.UnitTests.Application;

public class GetPortfolioQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenPortfolioExists_ReturnsPortfolioDto()
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddFunds(1000m);
        portfolio.AddHolding("AAPL", 10, 150m);

        var repository = new StubPortfolioRepository(portfolio);
        var handler = new GetPortfolioQueryHandler(repository);

        var result = await handler.Handle(new GetPortfolioQuery("user-1"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(portfolio.Id);
        result.UserId.Should().Be("user-1");
        result.CashBalance.Should().Be(1000m);
        result.Holdings.Should().HaveCount(1);
        result.Holdings[0].Symbol.Should().Be("AAPL");
        result.Holdings[0].Quantity.Should().Be(10);
        result.Holdings[0].AveragePrice.Should().Be(150m);
    }

    [Fact]
    public async Task Handle_WhenEmptyPortfolio_ReturnsZeroCashAndEmptyHoldings()
    {
        var portfolio = Portfolio.Create("user-1");

        var repository = new StubPortfolioRepository(portfolio);
        var handler = new GetPortfolioQueryHandler(repository);

        var result = await handler.Handle(new GetPortfolioQuery("user-1"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.CashBalance.Should().Be(0);
        result.Holdings.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_WhenPortfolioNotFound_ReturnsNull()
    {
        var repository = new StubPortfolioRepository(null);
        var handler = new GetPortfolioQueryHandler(repository);

        var result = await handler.Handle(new GetPortfolioQuery("unknown-user"), CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithMultipleHoldings_MapsAllCorrectly()
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddHolding("AAPL", 5, 100m);
        portfolio.AddHolding("MSFT", 3, 200m);

        var repository = new StubPortfolioRepository(portfolio);
        var handler = new GetPortfolioQueryHandler(repository);

        var result = await handler.Handle(new GetPortfolioQuery("user-1"), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Holdings.Should().HaveCount(2);
        result.Holdings.Should().Contain(h => h.Symbol == "AAPL" && h.Quantity == 5 && h.AveragePrice == 100m);
        result.Holdings.Should().Contain(h => h.Symbol == "MSFT" && h.Quantity == 3 && h.AveragePrice == 200m);
    }

    private class StubPortfolioRepository : IPortfolioRepository
    {
        private readonly Portfolio? _portfolio;

        public StubPortfolioRepository(Portfolio? portfolio)
        {
            _portfolio = portfolio;
        }

        public Task<Portfolio?> GetByUserAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_portfolio);

        public Task AddAsync(Portfolio portfolio, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Portfolio portfolio, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
