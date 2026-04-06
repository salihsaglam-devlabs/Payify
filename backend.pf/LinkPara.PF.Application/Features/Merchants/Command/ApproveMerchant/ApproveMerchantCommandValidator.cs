using FluentValidation;
using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Features.Merchants.Command.ApproveMerchant;

public class ApproveMerchantCommandValidator : AbstractValidator<ApproveMerchantCommand>
{
    public ApproveMerchantCommandValidator()
    {
        RuleFor(b => b.MerchantId).NotEmpty().NotNull();

        When(b => b.MerchantStatus == MerchantStatus.Reject, () =>
        {
            RuleFor(b => b.RejectReason).NotNull().NotEmpty()
            .WithMessage("Reject reason not null!");
        });
    }
}
