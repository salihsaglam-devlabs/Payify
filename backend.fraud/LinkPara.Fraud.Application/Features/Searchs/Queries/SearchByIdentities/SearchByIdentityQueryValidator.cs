using FluentValidation;

namespace LinkPara.Fraud.Application.Features.Searchs.SearchByIdentities;

public class SearchByIdentityQueryValidator : AbstractValidator<SearchByIdentityQuery>
{
    public SearchByIdentityQueryValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
        RuleFor(x => x.Id).MinimumLength(5);
    }
}
