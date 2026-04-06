using FluentValidation;

namespace LinkPara.PF.Application.Features.Posting.Queries;

public class PostingTransferErrorQueryValidator : AbstractValidator<PostingTransferErrorQuery>
{
    public PostingTransferErrorQueryValidator()
    {
        RuleFor(r => r.MerchantId).NotEqual(Guid.Empty);
        RuleFor(r => r.MerchantTransactionId).NotEqual(Guid.Empty);
        RuleFor(r => r.PostingDateStart).NotEqual(DateTime.MinValue);
        RuleFor(r => r.PostingDateEnd).NotEqual(DateTime.MinValue);
        RuleFor(r => r.TransactionStartDate).NotEqual(DateTime.MinValue);
        RuleFor(r => r.TransactionEndDate).NotEqual(DateTime.MinValue);
    }
}