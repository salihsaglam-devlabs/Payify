using FluentValidation;

namespace LinkPara.Approval.Application.Features.MakerCheckers.Commands.SaveMakerChecker;

public class SaveMakerCheckerCommandValidator : AbstractValidator<SaveMakerCheckerCommand>
{
    public SaveMakerCheckerCommandValidator()
    {
        RuleFor(x => x.CaseId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.MakerRoleId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.CheckerRoleId)
            .NotNull()
            .NotEmpty();
    }
}
