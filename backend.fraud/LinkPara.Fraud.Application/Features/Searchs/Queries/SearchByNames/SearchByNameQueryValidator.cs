using FluentValidation;

namespace LinkPara.Fraud.Application.Features.Searchs.SearchByNames;

public class SearchByNameQueryValidator : AbstractValidator<SearchByNameQuery>
{
    public SearchByNameQueryValidator()
    {
        RuleFor(x => x.Name).NotNull().NotEmpty();
        RuleFor(x => x.Name).MinimumLength(3);
    }
}
