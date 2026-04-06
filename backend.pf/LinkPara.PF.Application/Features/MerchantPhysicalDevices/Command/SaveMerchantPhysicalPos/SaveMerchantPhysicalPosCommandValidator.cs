using FluentValidation;

namespace LinkPara.PF.Application.Features.MerchantPhysicalDevices.Command.SaveMerchantPhysicalPos;

public class SaveMerchantPhysicalPosCommandValidator : AbstractValidator<SaveMerchantPhysicalPosCommand>
{
    public SaveMerchantPhysicalPosCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();

        RuleFor(x => x.PhysicalPosIdList)
             .NotEmpty().NotEmpty()
             .WithMessage("Physical Pos cannot be empty");
    }
}
