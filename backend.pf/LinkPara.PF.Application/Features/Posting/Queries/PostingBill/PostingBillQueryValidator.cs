using FluentValidation;

namespace LinkPara.PF.Application.Features.Posting.Queries;

public class PostingBillQueryValidator : AbstractValidator<PostingBillQuery>
{
    public PostingBillQueryValidator()
    {
        RuleFor(r => r.MerchantId).NotEqual(Guid.Empty);
        RuleFor(r => r.StartDate).NotEqual(DateTime.MinValue);
        RuleFor(r => r.EndDate).NotEqual(DateTime.MinValue);
        RuleFor(r => r.MinAmount).GreaterThanOrEqualTo(0);
        RuleFor(r => r.BillMonth).GreaterThanOrEqualTo(0).LessThanOrEqualTo(12);
    }
}