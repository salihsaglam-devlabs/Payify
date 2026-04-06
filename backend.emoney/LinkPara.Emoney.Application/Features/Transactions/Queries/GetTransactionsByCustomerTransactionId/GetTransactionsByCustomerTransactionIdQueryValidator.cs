using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetTransactionsByCustomerTransactionId;

public class GetTransactionsByCustomerTransactionIdQueryValidator : AbstractValidator<GetTransactionsByCustomerTransactionIdQuery>
{
    public GetTransactionsByCustomerTransactionIdQueryValidator()
    {
        RuleFor(x => x.CustomerTransactionId)
            .NotEmpty()
            .MaximumLength(50);
    }
}
