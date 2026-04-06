using FluentValidation;

namespace LinkPara.PF.Application.Features.VirtualPos.Command.DeleteVpos;

public class DeleteVposCommandValidator : AbstractValidator<DeleteVposCommand>
{
    public DeleteVposCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
