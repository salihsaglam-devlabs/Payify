using FluentValidation;

namespace LinkPara.Approval.Application.Features.Cases.Commands.DeleteCase;

public class DeleteCaseCommandValidator : AbstractValidator<DeleteCaseCommand>
{
    public DeleteCaseCommandValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
    }
}
