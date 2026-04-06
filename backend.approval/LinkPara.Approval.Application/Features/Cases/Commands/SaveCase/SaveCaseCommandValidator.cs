using FluentValidation;

namespace LinkPara.Approval.Application.Features.Cases.Commands.SaveCase;

public class SaveCaseCommandValidator : AbstractValidator<SaveCaseCommand>
{
    public SaveCaseCommandValidator()
    {
        RuleFor(x => x.BaseUrl)
            .NotNull()
            .NotEmpty()
            .Length(100);
        RuleFor(x => x.Resource)
            .NotNull()
            .NotEmpty()
            .Length(100);
        RuleFor(x => x.DisplayName)
            .NotNull()
            .NotEmpty()
            .Length(100);
    }
}
