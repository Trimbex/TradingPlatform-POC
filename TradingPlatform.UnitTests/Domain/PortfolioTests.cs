using FluentAssertions;
using TradingPlatform.Domain.Entities;
using Xunit;

namespace TradingPlatform.UnitTests.Domain;

public class PortfolioTests
{
    [Fact]
    public void Create_WithValidUserId_StartsWithZeroCash()
    {
        var portfolio = Portfolio.Create("user-1");

        portfolio.Should().NotBeNull();
        portfolio.Id.Should().NotBe(Guid.Empty);
        portfolio.UserId.Should().Be("user-1");
        portfolio.CashBalance.Should().Be(0);
        portfolio.Holdings.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithEmptyUserId_ThrowsArgumentException(string? userId)
    {
        var act = () => Portfolio.Create(userId!);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("userId");
    }

    [Fact]
    public void AddFunds_IncreasesCashBalance()
    {
        var portfolio = Portfolio.Create("user-1");

        portfolio.AddFunds(1000m);

        portfolio.CashBalance.Should().Be(1000m);
    }

    [Fact]
    public void AddFunds_CanBeCalledMultipleTimes()
    {
        var portfolio = Portfolio.Create("user-1");

        portfolio.AddFunds(500m);
        portfolio.AddFunds(300m);
        portfolio.AddFunds(200m);

        portfolio.CashBalance.Should().Be(1000m);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void AddFunds_WithZeroOrNegativeAmount_ThrowsArgumentException(decimal amount)
    {
        var portfolio = Portfolio.Create("user-1");

        var act = () => portfolio.AddFunds(amount);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("amount");
    }

    [Fact]
    public void Withdraw_DecreasesCashBalance()
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddFunds(1000m);

        portfolio.Withdraw(300m);

        portfolio.CashBalance.Should().Be(700m);
    }

    [Fact]
    public void Withdraw_ExcessiveAmount_ThrowsInvalidOperationException()
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddFunds(100m);

        var act = () => portfolio.Withdraw(150m);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Insufficient funds*")
            .WithMessage("*100*")
            .WithMessage("*150*");
    }

    [Fact]
    public void Withdraw_MoreThanBalance_ThrowsInvalidOperationException()
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddFunds(50m);

        var act = () => portfolio.Withdraw(50.01m);

        act.Should().Throw<InvalidOperationException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Withdraw_WithZeroOrNegativeAmount_ThrowsArgumentException(decimal amount)
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddFunds(100m);

        var act = () => portfolio.Withdraw(amount);

        act.Should().Throw<ArgumentException>()
            .WithParameterName("amount");
    }

    [Fact]
    public void Withdraw_ExactBalance_Succeeds()
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddFunds(100m);

        portfolio.Withdraw(100m);

        portfolio.CashBalance.Should().Be(0);
    }

    [Fact]
    public void AddHolding_AddsNewHolding()
    {
        var portfolio = Portfolio.Create("user-1");

        portfolio.AddHolding("AAPL", 10, 150m);

        portfolio.Holdings.Should().HaveCount(1);
        portfolio.Holdings[0].Symbol.Should().Be("AAPL");
        portfolio.Holdings[0].Quantity.Should().Be(10);
        portfolio.Holdings[0].AveragePrice.Should().Be(150m);
    }

    [Fact]
    public void AddHolding_SameSymbol_UpdatesExistingHoldingWithWeightedAverage()
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddHolding("AAPL", 10, 100m);

        portfolio.AddHolding("AAPL", 10, 200m);

        portfolio.Holdings.Should().HaveCount(1);
        portfolio.Holdings[0].Symbol.Should().Be("AAPL");
        portfolio.Holdings[0].Quantity.Should().Be(20);
        portfolio.Holdings[0].AveragePrice.Should().Be(150m); // (10*100 + 10*200) / 20
    }

    [Fact]
    public void RemoveHolding_ReducesQuantity()
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddHolding("AAPL", 10, 150m);

        portfolio.RemoveHolding("AAPL", 5);

        portfolio.Holdings.Should().HaveCount(1);
        portfolio.Holdings[0].Quantity.Should().Be(5);
    }

    [Fact]
    public void RemoveHolding_AllQuantity_RemovesHolding()
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddHolding("AAPL", 10, 150m);

        portfolio.RemoveHolding("AAPL", 10);

        portfolio.Holdings.Should().BeEmpty();
    }

    [Fact]
    public void RemoveHolding_NonExistentSymbol_ThrowsInvalidOperationException()
    {
        var portfolio = Portfolio.Create("user-1");

        var act = () => portfolio.RemoveHolding("MSFT", 5);

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*MSFT*");
    }

    [Fact]
    public void AddHolding_IsCaseInsensitive()
    {
        var portfolio = Portfolio.Create("user-1");
        portfolio.AddHolding("aapl", 10, 100m);

        portfolio.AddHolding("AAPL", 10, 200m);

        portfolio.Holdings.Should().HaveCount(1);
        portfolio.Holdings[0].Quantity.Should().Be(20);
    }
}
