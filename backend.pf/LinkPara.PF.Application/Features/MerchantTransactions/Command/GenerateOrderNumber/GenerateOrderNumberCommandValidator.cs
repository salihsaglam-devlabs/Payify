using FluentValidation;


namespace LinkPara.PF.Application.Features.MerchantTransactions.Command.GenerateOrderNumber;

public class GenerateOrderNumberCommandValidator : AbstractValidator<GenerateOrderNumberCommand>
{
    public GenerateOrderNumberCommandValidator()
    {
        RuleFor(x => x.MerchantId)
       .NotNull().NotEmpty();

        When(x => x.OrderNumber is not null, () =>
        {
            RuleFor(x => x.OrderNumber)
                .NotNull().NotEmpty()
                .Matches("^[a-zA-Z0-9]*$").WithMessage("OrderNumber sadece harf ve rakam içerebilir.")
                .MaximumLength(24);
        });
    }
}
