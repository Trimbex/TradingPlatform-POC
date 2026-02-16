using FluentAssertions;
using TradingPlatform.Domain.Entities;
using TradingPlatform.Domain.Enums;
using Xunit;

namespace TradingPlatform.UnitTests.Domain;

public class OrderTests
{
    [Fact]
    public void Create_WithValidData_ReturnsOrderWithPendingStatus()
    {
        var order = Order.Create("user-1", "AAPL", 10, 150.50m);

        order.Should().NotBeNull();
        order.Id.Should().NotBe(Guid.Empty);
        order.UserId.Should().Be("user-1");
        order.Symbol.Should().Be("AAPL");
        order.Quantity.Should().Be(10);
        order.Price.Should().Be(150.50m);
        order.Status.Should().Be(OrderStatus.Pending);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_TrimsSymbol()
    {
        var order = Order.Create("user-1", "  AAPL  ", 10, 150.50m);

        order.Symbol.Should().Be("AAPL");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyUserId_ThrowsArgumentException(string? userId)
    {
        var act = () => Order.Create(userId!, "AAPL", 10, 150.50m);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("userId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptySymbol_ThrowsArgumentException(string? symbol)
    {
        var act = () => Order.Create("user-1", symbol!, 10, 150.50m);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("symbol");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeQuantity_ThrowsArgumentException(decimal quantity)
    {
        var act = () => Order.Create("user-1", "AAPL", quantity, 150.50m);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("quantity");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativePrice_ThrowsArgumentException(decimal price)
    {
        var act = () => Order.Create("user-1", "AAPL", 10, price);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("price");
    }

    [Fact]
    public void Execute_WhenPending_ChangesStatusToExecuted()
    {
        var order = Order.Create("user-1", "AAPL", 10, 150.50m);

        order.Execute();

        order.Status.Should().Be(OrderStatus.Executed);
    }

    [Fact]
    public void Execute_WhenAlreadyExecuted_ThrowsInvalidOperationException()
    {
        var order = Order.Create("user-1", "AAPL", 10, 150.50m);
        order.Execute();

        var act = () => order.Execute();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Executed*");
    }

    [Fact]
    public void Execute_WhenCancelled_ThrowsInvalidOperationException()
    {
        var order = Order.Create("user-1", "AAPL", 10, 150.50m);
        order.Cancel();

        var act = () => order.Execute();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cancelled*");
    }

    [Fact]
    public void Cancel_WhenPending_ChangesStatusToCancelled()
    {
        var order = Order.Create("user-1", "AAPL", 10, 150.50m);

        order.Cancel();

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_WhenAlreadyExecuted_ThrowsInvalidOperationException()
    {
        var order = Order.Create("user-1", "AAPL", 10, 150.50m);
        order.Execute();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Executed*");
    }

    [Fact]
    public void Cancel_WhenAlreadyCancelled_ThrowsInvalidOperationException()
    {
        var order = Order.Create("user-1", "AAPL", 10, 150.50m);
        order.Cancel();

        var act = () => order.Cancel();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cancelled*");
    }
}
