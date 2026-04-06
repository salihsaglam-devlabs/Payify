using FluentValidation;

namespace LinkPara.Epin.Application.Features.Orders.Queries.GetUserOrdersFilter;

public class GetUserOrdersFilterQueryValidator : AbstractValidator<GetUserOrdersFilterQuery>
{
    public GetUserOrdersFilterQueryValidator()
    {
        RuleFor(x => x.UserId)
           .NotNull().NotEmpty();
    }
}
