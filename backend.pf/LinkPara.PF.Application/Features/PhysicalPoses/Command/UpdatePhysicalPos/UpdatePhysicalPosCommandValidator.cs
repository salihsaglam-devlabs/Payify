using FluentValidation;

namespace LinkPara.PF.Application.Features.PhysicalPoses.Command.UpdatePhysicalPos;

public class UpdatePhysicalPosCommandValidator : AbstractValidator<UpdatePhysicalPosCommand>
{
    public UpdatePhysicalPosCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(100)
 .WithMessage("Invalid Physical Pos name!");

        RuleFor(x => x.PfMainMerchantId).MaximumLength(100)
      .WithMessage("Invalid PfMainMerchantId!");

        RuleFor(x => x.AcquireBankId).NotNull().NotEmpty()
            .WithMessage("Acquire Bank cant be empty!");
    }
}
