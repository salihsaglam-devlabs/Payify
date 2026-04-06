using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Transactions.Queries.GetReceipt;

public class GetReceiptQueryValidator : AbstractValidator<GetReceiptQuery>
{
    public GetReceiptQueryValidator()
    {
        RuleFor(x => x.TransactionId)
            .NotNull()
            .NotEmpty();
    }
}