using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.PaymentOrderDetail;

public class PaymentOrderDetailQueryValidator : AbstractValidator<PaymentOrderDetailQuery>
{
    public PaymentOrderDetailQueryValidator()
    {
        RuleFor(x => x.ConsentId).NotEmpty().NotNull();
        RuleFor(x => x.HhsCode).NotEmpty().NotNull();
        RuleFor(x => x.AppUserId).NotEmpty().NotNull();
    }
}
