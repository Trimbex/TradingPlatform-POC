using FluentAssertions;
using TradingPlatform.Application.DTOs;
using TradingPlatform.Application.Queries;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Enums;
using TradingPlatform.Domain.Interfaces;
using Xunit;

namespace TradingPlatform.UnitTests.Application;

public class GetOrderQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenOrderExists_ReturnsOrderDto()
    {
        var order = Order.Create("user-1", "AAPL", 10, 150.50m);
        var repository = new StubOrderRepository(order);
        var handler = new GetOrderQueryHandler(repository);

        var result = await handler.Handle(new GetOrderQuery(order.Id), CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.UserId.Should().Be("user-1");
        result.Symbol.Should().Be("AAPL");
        result.Quantity.Should().Be(10);
        result.Price.Should().Be(150.50m);
        result.Status.Should().Be(OrderStatus.Pending);
        result.CreatedAt.Should().Be(order.CreatedAt);
    }

    [Fact]
    public async Task Handle_WhenOrderNotFound_ReturnsNull()
    {
        var repository = new StubOrderRepository(null);
        var handler = new GetOrderQueryHandler(repository);

        var result = await handler.Handle(new GetOrderQuery(Guid.NewGuid()), CancellationToken.None);

        result.Should().BeNull();
    }

    private class StubOrderRepository : IOrderRepository
    {
        private readonly Order? _order;

        public StubOrderRepository(Order? order)
        {
            _order = order;
        }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_order?.Id == id ? _order : null);

        public Task<IEnumerable<Order>> GetByUserAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(Enumerable.Empty<Order>());

        public Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Order order, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
