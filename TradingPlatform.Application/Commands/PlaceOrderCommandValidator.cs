using FluentValidation;

namespace TradingPlatform.Application.Commands;

public class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId cannot be empty.");

        RuleFor(x => x.Symbol)
            .NotEmpty().WithMessage("Symbol cannot be empty.")
            .Length(1, 5).WithMessage("Symbol must be between 1 and 5 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than zero.");
    }
}
