using FluentValidation;

namespace LinkPara.Epin.Application.Features.Orders.Queries.GetOrderSummary;

public class GetOrderSummaryQueryValidator : AbstractValidator<GetOrderSummaryQuery>
{
    public GetOrderSummaryQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotNull().NotEmpty();
    }
}