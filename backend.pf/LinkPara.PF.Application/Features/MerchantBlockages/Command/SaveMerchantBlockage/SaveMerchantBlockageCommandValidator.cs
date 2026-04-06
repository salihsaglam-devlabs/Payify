using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantBlockages.Command.SaveMerchantBlockage;

public class SaveMerchantBlockageCommandValidator : AbstractValidator<SaveMerchantBlockageCommand>
{
    public SaveMerchantBlockageCommandValidator()
    {
        RuleFor(b => b.MerchantId).NotNull().NotEmpty();
        RuleFor(b => b.MerchantBlockageStatus).IsInEnum();
        RuleFor(b => b.TotalAmount).GreaterThanOrEqualTo(0);
        RuleFor(b => b.BlockageAmount).GreaterThanOrEqualTo(0);
        RuleFor(b => b.RemainingAmount).GreaterThanOrEqualTo(0);
    }
}