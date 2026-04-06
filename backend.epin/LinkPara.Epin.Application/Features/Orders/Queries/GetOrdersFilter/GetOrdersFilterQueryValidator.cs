using FluentValidation;

namespace LinkPara.Epin.Application.Features.Orders.Queries.GetOrdersFilter;

public class GetOrdersFilterQueryValidator : AbstractValidator<GetOrdersFilterQuery>
{
    public GetOrdersFilterQueryValidator()
    {
        RuleFor(b => b.CreateDateEnd)
            .GreaterThan(b => b.CreateDateStart.Value)
            .WithMessage("End date must after Start date!")
            .When(b => b.CreateDateStart.HasValue);

        RuleFor(b => b.ProductId)
            .GreaterThan(0)
            .WithMessage("Brand must chosen!")
            .When(b => b.ProductId.HasValue);
    }
}
