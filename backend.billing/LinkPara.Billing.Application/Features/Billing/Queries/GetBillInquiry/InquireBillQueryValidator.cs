using FluentValidation;
using LinkPara.Billing.Application.Features.InstitutionApis.Queries.GetBillInquiry;

namespace LinkPara.Billing.Application.Features.Billing.Queries.GetBillInquiry;

public class InquireBillQueryValidator : AbstractValidator<InquireBillQuery>
{
    public InquireBillQueryValidator()
    {
        RuleFor(s => s.InstitutionId).NotEmpty().NotNull();
        RuleFor(s => s.SubscriberNumber1).NotEmpty().NotNull();
    }
}