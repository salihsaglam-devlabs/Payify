using FluentValidation;

namespace LinkPara.Approval.Application.Features.MakerCheckers.Commands.UpdateMakerChecker;

public class UpdateMakerCheckerCommandValidator : AbstractValidator<UpdateMakerCheckerCommand>
{
    public UpdateMakerCheckerCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .NotEmpty();
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