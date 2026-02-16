using FluentAssertions;
using TradingPlatform.Domain.Entities;
using Xunit;

namespace TradingPlatform.UnitTests.Domain;

public class HoldingTests
{
    [Fact]
    public void Create_WithValidData_ReturnsHolding()
    {
        var holding = Holding.Create("AAPL", 10, 150.50m);

        holding.Should().NotBeNull();
        holding.Symbol.Should().Be("AAPL");
        holding.Quantity.Should().Be(10);
        holding.AveragePrice.Should().Be(150.50m);
    }

    [Fact]
    public void Create_TrimsSymbol()
    {
        var holding = Holding.Create("  AAPL  ", 10, 150m);

        holding.Symbol.Should().Be("AAPL");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptySymbol_ThrowsArgumentException(string? symbol)
    {
        var act = () => Holding.Create(symbol!, 10, 150m);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("symbol");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativeQuantity_ThrowsArgumentException(decimal quantity)
    {
        var act = () => Holding.Create("AAPL", quantity, 150m);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("quantity");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_WithZeroOrNegativePrice_ThrowsArgumentException(decimal price)
    {
        var act = () => Holding.Create("AAPL", 10, price);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("price");
    }

    [Fact]
    public void AddQuantity_RecalculatesWeightedAverage()
    {
        var holding = Holding.Create("AAPL", 10, 100m);

        holding.AddQuantity(10, 200m);

        holding.Quantity.Should().Be(20);
        holding.AveragePrice.Should().Be(150m); // (10*100 + 10*200) / 20
    }

    [Fact]
    public void AddQuantity_SamePrice_KeepsAverage()
    {
        var holding = Holding.Create("AAPL", 10, 100m);

        holding.AddQuantity(5, 100m);

        holding.Quantity.Should().Be(15);
        holding.AveragePrice.Should().Be(100m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddQuantity_WithZeroOrNegativeQuantity_ThrowsArgumentException(decimal quantity)
    {
        var holding = Holding.Create("AAPL", 10, 100m);

        var act = () => holding.AddQuantity(quantity, 100m);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("quantity");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddQuantity_WithZeroOrNegativePrice_ThrowsArgumentException(decimal price)
    {
        var holding = Holding.Create("AAPL", 10, 100m);

        var act = () => holding.AddQuantity(5, price);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("price");
    }

    [Fact]
    public void RemoveQuantity_ReducesQuantity()
    {
        var holding = Holding.Create("AAPL", 10, 150m);

        holding.RemoveQuantity(4);

        holding.Quantity.Should().Be(6);
    }

    [Fact]
    public void RemoveQuantity_AllQuantity_LeavesZero()
    {
        var holding = Holding.Create("AAPL", 10, 150m);

        holding.RemoveQuantity(10);

        holding.Quantity.Should().Be(0);
    }

    [Fact]
    public void RemoveQuantity_ExcessiveQuantity_ThrowsInvalidOperationException()
    {
        var holding = Holding.Create("AAPL", 10, 150m);

        var act = () => holding.RemoveQuantity(15);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*15*")
            .WithMessage("*10*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RemoveQuantity_WithZeroOrNegativeQuantity_ThrowsArgumentException(decimal quantity)
    {
        var holding = Holding.Create("AAPL", 10, 150m);

        var act = () => holding.RemoveQuantity(quantity);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("quantity");
    }

    [Fact]
    public void HasQuantity_WhenSufficient_ReturnsTrue()
    {
        var holding = Holding.Create("AAPL", 10, 150m);

        holding.HasQuantity(5).Should().BeTrue();
        holding.HasQuantity(10).Should().BeTrue();
    }

    [Fact]
    public void HasQuantity_WhenInsufficient_ReturnsFalse()
    {
        var holding = Holding.Create("AAPL", 10, 150m);

        holding.HasQuantity(11).Should().BeFalse();
    }
}
