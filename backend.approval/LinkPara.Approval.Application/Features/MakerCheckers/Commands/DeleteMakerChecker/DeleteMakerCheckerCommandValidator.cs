using FluentValidation;

namespace LinkPara.Approval.Application.Features.MakerCheckers.Commands.DeleteMakerChecker;

public class DeleteMakerCheckerCommandValidator : AbstractValidator<DeleteMakerCheckerCommand>
{
    public DeleteMakerCheckerCommandValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
    }
}

