using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetPaymentOrderConsentDetail;

public class GetPaymentOrderConsentDetailQueryValidator : AbstractValidator<GetPaymentOrderConsentDetailQuery>
{
    public GetPaymentOrderConsentDetailQueryValidator()
    {
        RuleFor(x => x.ConsentId).NotEmpty().NotNull();
        RuleFor(x => x.HhsCode).NotEmpty().NotNull();
        RuleFor(x => x.AppUserId).NotEmpty().NotNull();
    }
}
