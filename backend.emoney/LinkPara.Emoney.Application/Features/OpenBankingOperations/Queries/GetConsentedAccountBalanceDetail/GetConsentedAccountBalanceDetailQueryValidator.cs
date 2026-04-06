using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountBalanceDetail;

public class GetConsentedAccountBalanceDetailQueryValidator : AbstractValidator<GetConsentedAccountBalanceDetailQuery>
{
    public GetConsentedAccountBalanceDetailQueryValidator()
    {
        RuleFor(x => x.AccountReference).NotEmpty().NotNull();
        RuleFor(x => x.HhsCode).NotEmpty().NotNull();
        RuleFor(x => x.AppUserId).NotEmpty().NotNull();
    }
}
