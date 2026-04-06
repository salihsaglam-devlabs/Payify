using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantBlockages.Command.UpdateMerchantBlockage;

public class UpdateMerchantBlockageCommandValidator : AbstractValidator<UpdateMerchantBlockageCommand>
{
    public UpdateMerchantBlockageCommandValidator()
    {
        RuleFor(b => b.Id).NotNull().NotEmpty();
        RuleFor(b => b.TotalAmount).GreaterThanOrEqualTo(0);
    }
}
