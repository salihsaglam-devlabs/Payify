using FluentValidation;

namespace LinkPara.Accounting.Application.Features.Payments.Queries.GetFilterPayment;

internal class GetFilterPaymentQueryValidator : AbstractValidator<GetFilterPaymentQuery>
{
    public GetFilterPaymentQueryValidator()
    {
        RuleFor(b => b.TransactionDateEnd)
                .GreaterThan(b => b.TransactionDateStart.Value)
                .WithMessage("End date must after Start date!")
                .When(b => b.TransactionDateStart.HasValue);
    }
}