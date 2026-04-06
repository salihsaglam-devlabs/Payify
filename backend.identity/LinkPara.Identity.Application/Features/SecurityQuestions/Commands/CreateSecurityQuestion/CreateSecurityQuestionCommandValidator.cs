using FluentValidation;

namespace LinkPara.Identity.Application.Features.SecurityQuestions.Commands.CreateSecurityQuestion;

public class CreateSecurityQuestionCommandValidator : AbstractValidator<CreateSecurityQuestionCommand>
{
    public CreateSecurityQuestionCommandValidator()
    {
        RuleFor(u => u.Question)
            .NotNull().NotEmpty().MaximumLength(100);
    }
}