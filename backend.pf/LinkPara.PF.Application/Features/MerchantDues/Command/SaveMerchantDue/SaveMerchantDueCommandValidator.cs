using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantDues.Command.SaveMerchantDue;

public class SaveMerchantDueCommandValidator : AbstractValidator<SaveMerchantDueCommand>
{
    public SaveMerchantDueCommandValidator()
    {
        RuleFor(x => x.MerchantId)
            .NotNull().NotEmpty().WithMessage("MerchantId cant be empty!");
        RuleFor(x => x.DueProfileId)
            .NotNull().NotEmpty().WithMessage("DueProfileId cant be empty!");
    }
}