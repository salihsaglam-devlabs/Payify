
using FluentValidation;

namespace LinkPara.Identity.Application.Features.SecurityQuestions.Commands.UpdateSecurityQuestion;
public class UpdateSecurityQuestionCommandValidator : AbstractValidator<UpdateSecurityQuestionCommand>
{
    public UpdateSecurityQuestionCommandValidator()
    {
        RuleFor(u => u.Id)
            .NotNull()
            .NotEmpty();

        RuleFor(u => u.Question)
            .NotNull()
            .NotEmpty();
    }
}