using FluentValidation;

namespace LinkPara.Epin.Application.Features.Orders.Commands.CancelOrder;

public class CancelOrderCommandValidator : AbstractValidator<CancelOrderCommand>
{
    public CancelOrderCommandValidator()
    {
        RuleFor(x => x.OrderId)
            .NotNull().NotEmpty();
    }
}
