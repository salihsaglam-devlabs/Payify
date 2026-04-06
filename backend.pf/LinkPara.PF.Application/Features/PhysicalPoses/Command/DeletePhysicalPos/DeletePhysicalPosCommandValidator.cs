using FluentValidation;

namespace LinkPara.PF.Application.Features.PhysicalPoses.Command.DeletePhysicalPos;

public class DeletePhysicalPosCommandValidator : AbstractValidator<DeletePhysicalPosCommand>
{
    public DeletePhysicalPosCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
