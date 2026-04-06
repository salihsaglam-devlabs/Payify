using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdatePaymentDate;

public class UpdatePaymentDateCommandValidator : AbstractValidator<UpdatePaymentDateCommand>
{
	public UpdatePaymentDateCommandValidator()
	{
        RuleFor(b => b.PostBalanceId).NotNull().NotEmpty();
        RuleFor(b => b.PaymentDate).NotNull().NotEmpty();
    }
}
