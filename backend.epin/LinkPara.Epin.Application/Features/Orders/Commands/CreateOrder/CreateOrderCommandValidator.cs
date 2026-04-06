using FluentValidation;

namespace LinkPara.Epin.Application.Features.Orders.Commands.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.BrandId)
            .NotNull().NotEmpty();
        RuleFor(x => x.PublisherId)
            .NotNull().NotEmpty();
        RuleFor(x => x.WalletNumber)
            .NotNull().NotEmpty();
        RuleFor(x => x.Amount)
            .GreaterThan(0).NotEmpty();
        RuleFor(x => x.ProductId)
            .GreaterThan(0).NotEmpty();
    }
}