using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountBalanceList;

public class GetConsentedAccountBalanceListQueryValidator : AbstractValidator<GetConsentedAccountBalanceListQuery>
{
    public GetConsentedAccountBalanceListQueryValidator()
    {
        RuleFor(x => x.HhsCode).NotEmpty().NotNull();
        RuleFor(x => x.AppUserId).NotEmpty().NotNull();
    }
}
