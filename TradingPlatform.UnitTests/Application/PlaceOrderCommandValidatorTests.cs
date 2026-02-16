using FluentAssertions;
using FluentValidation;
using TradingPlatform.Application.Commands;
using Xunit;

namespace TradingPlatform.UnitTests.Application;

public class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_Passes()
    {
        var command = new PlaceOrderCommand("user-1", "AAPL", 10, 150.50m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyUserId_Fails(string? userId)
    {
        var command = new PlaceOrderCommand(userId!, "AAPL", 10, 150.50m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptySymbol_Fails(string? symbol)
    {
        var command = new PlaceOrderCommand("user-1", symbol!, 10, 150.50m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Symbol");
    }

    [Fact]
    public void Validate_WithSymbolTooLong_Fails()
    {
        var command = new PlaceOrderCommand("user-1", "TOOLONG", 10, 150.50m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Symbol" && e.ErrorMessage.Contains("1 and 5"));
    }

    [Fact]
    public void Validate_WithSymbolExactly5Characters_Passes()
    {
        var command = new PlaceOrderCommand("user-1", "ABCDE", 10, 150.50m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithZeroOrNegativeQuantity_Fails(decimal quantity)
    {
        var command = new PlaceOrderCommand("user-1", "AAPL", quantity, 150.50m);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithZeroOrNegativePrice_Fails(decimal price)
    {
        var command = new PlaceOrderCommand("user-1", "AAPL", 10, price);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

}
