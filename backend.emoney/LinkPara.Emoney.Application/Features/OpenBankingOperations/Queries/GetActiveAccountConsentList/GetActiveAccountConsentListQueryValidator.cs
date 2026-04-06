using FluentValidation;

namespace LinkPara.Emoney.Application.Features.OpenBankingOperations.Queries.GetActiveAccountConsentList;

public class GetActiveAccountConsentListQueryValidator : AbstractValidator<GetActiveAccountConsentListQuery>
{
    public GetActiveAccountConsentListQueryValidator()
    {
        RuleFor(x => x.AppUserId).NotEmpty().NotNull();
    }
}
