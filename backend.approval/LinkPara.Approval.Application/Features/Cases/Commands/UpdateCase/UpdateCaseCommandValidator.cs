using FluentValidation;

namespace LinkPara.Approval.Application.Features.Cases.Commands.UpdateCase;

public class UpdateCaseCommandValidator : AbstractValidator<UpdateCaseCommand>
{
    public UpdateCaseCommandValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
    }
}
