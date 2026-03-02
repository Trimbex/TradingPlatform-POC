using FluentValidation;

namespace TradingPlatform.Application.Commands;

public class ExecuteOrderCommandValidator : AbstractValidator<ExecuteOrderCommand>
{
    public ExecuteOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty().WithMessage("OrderId cannot be empty.");
    }
}
