using FluentValidation;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Queries.PaymentOrderInquiry;

public class PaymentOrderInquiryQueryValidator : AbstractValidator<PaymentOrderInquiryQuery>
{
    public PaymentOrderInquiryQueryValidator()
    {
        RuleFor(x => x.PaymentGuid).NotEmpty().NotNull();
    }
}
