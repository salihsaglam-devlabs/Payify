using FluentValidation;

namespace LinkPara.Emoney.Application.Features.ConsentOperations.Queries.GetActiveConsentList;

public class GetActiveConsentListQueryValidator : AbstractValidator<GetActiveConsentListQuery>
{
    public GetActiveConsentListQueryValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().NotNull();
        RuleFor(x => x.ConsentType).NotEmpty().NotNull();

    }
}
