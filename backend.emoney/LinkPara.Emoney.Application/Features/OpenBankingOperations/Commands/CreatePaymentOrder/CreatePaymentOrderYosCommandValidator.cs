using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Commands.CreatePaymentOrder;

public class CreatePaymentOrderYosCommandValidator : AbstractValidator<CreatePaymentOrderYosCommand>
{
    public CreatePaymentOrderYosCommandValidator()
    {
        RuleFor(x => x.ConsentId).NotEmpty().NotNull();
        RuleFor(x => x.HhsCode).NotEmpty().NotNull();
        RuleFor(x => x.AppUserId).NotEmpty().NotNull();
    }
}
