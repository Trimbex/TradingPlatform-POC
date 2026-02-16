using FluentAssertions;
using TradingPlatform.Application.Queries;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Enums;
using TradingPlatform.Domain.Interfaces;
using Xunit;

namespace TradingPlatform.UnitTests.Application;

public class GetOrdersQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenUserHasOrders_ReturnsAllOrders()
    {
        var order1 = Order.Create("user-1", "AAPL", 10, 150m);
        var order2 = Order.Create("user-1", "MSFT", 5, 200m);
        var repository = new StubOrderRepository([order1, order2]);
        var handler = new GetOrdersQueryHandler(repository);

        var result = await handler.Handle(new GetOrdersQuery("user-1"), CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().Contain(o => o.Symbol == "AAPL" && o.Quantity == 10);
        result.Should().Contain(o => o.Symbol == "MSFT" && o.Quantity == 5);
    }

    [Fact]
    public async Task Handle_WhenUserHasNoOrders_ReturnsEmptyList()
    {
        var repository = new StubOrderRepository([]);
        var handler = new GetOrdersQueryHandler(repository);

        var result = await handler.Handle(new GetOrdersQuery("user-1"), CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_MapsOrderStatusCorrectly()
    {
        var order = Order.Create("user-1", "AAPL", 10, 150m);
        order.Execute();
        var repository = new StubOrderRepository([order]);
        var handler = new GetOrdersQueryHandler(repository);

        var result = await handler.Handle(new GetOrdersQuery("user-1"), CancellationToken.None);

        result.Should().HaveCount(1);
        result[0].Status.Should().Be(OrderStatus.Executed);
    }

    private class StubOrderRepository : IOrderRepository
    {
        private readonly IReadOnlyList<Order> _orders;

        public StubOrderRepository(IReadOnlyList<Order> orders)
        {
            _orders = orders;
        }

        public Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_orders.FirstOrDefault(o => o.Id == id));

        public Task<IEnumerable<Order>> GetByUserAsync(string userId, CancellationToken cancellationToken = default) =>
            Task.FromResult(_orders.AsEnumerable());

        public Task AddAsync(Order order, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;

        public Task UpdateAsync(Order order, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }
}
