using FluentValidation;

namespace LinkPara.Billing.Application.Features.Billing.Queries.GetBillStatus;

public class GetBillStatusQueryValidator : AbstractValidator<GetBillStatusQuery>
{
    public GetBillStatusQueryValidator()
    {
        RuleFor(s => s.TransactionId).NotEmpty().NotNull().NotEqual(Guid.Empty);
    }
}