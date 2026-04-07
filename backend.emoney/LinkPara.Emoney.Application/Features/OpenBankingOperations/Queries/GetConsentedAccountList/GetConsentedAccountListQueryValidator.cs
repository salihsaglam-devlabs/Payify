using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetConsentedAccountList;

public class GetConsentedAccountListQueryValidator : AbstractValidator<GetConsentedAccountListQuery>
{
    public GetConsentedAccountListQueryValidator()
    {
        RuleFor(x => x.AppUserId).NotEmpty().NotNull();
        RuleFor(x => x.HhsCode).NotEmpty().NotNull();
    }
}
