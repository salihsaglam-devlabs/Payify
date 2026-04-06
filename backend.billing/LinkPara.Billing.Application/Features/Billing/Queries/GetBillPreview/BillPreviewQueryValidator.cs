using FluentValidation;

namespace LinkPara.Billing.Application.Features.Billing.Queries.GetBillPreview;

public class BillPreviewQueryValidator : AbstractValidator<BillPreviewQuery>
{
    public BillPreviewQueryValidator()
    {
        RuleFor(s => s.InstitutionId).NotEmpty().NotNull();
        RuleFor(s => s.RequestId).NotEmpty().NotNull();
        RuleFor(s => s.PayeeFullName).NotEmpty().NotNull();
        RuleFor(s => s.PayeeMobile).NotEmpty().NotNull();
        RuleFor(s => s.PayeeEmail).NotEmpty().NotNull();
        RuleFor(s => s.Bill.Id).NotEmpty().NotNull();
        RuleFor(s => s.Bill.Number).NotEmpty().NotNull();
        RuleFor(s => s.Bill.DueDate).NotEmpty().NotNull();
        RuleFor(s => s.Bill.Amount).NotEmpty().NotNull();
        RuleFor(s => s.Bill.Currency).NotEmpty().NotNull();
        RuleFor(s => s.Bill.SubscriberNumber1).NotEmpty().NotNull();
        RuleFor(s => s.Bill.SubscriberName).NotEmpty().NotNull();
    }
}
