using FluentValidation;

namespace LinkPara.Accounting.Application.Features.Customers.Queries.GetFilterCustomer;

public class GetFilterCustomerQueryValidator : AbstractValidator<GetFilterCustomerQuery>
{
    public GetFilterCustomerQueryValidator()
    {
        RuleFor(b => b.CreateDateEnd)
                .GreaterThan(b => b.CreateDateStart.Value)
                .WithMessage("End date must after Start date!")
                .When(b => b.CreateDateStart.HasValue);
    }
}