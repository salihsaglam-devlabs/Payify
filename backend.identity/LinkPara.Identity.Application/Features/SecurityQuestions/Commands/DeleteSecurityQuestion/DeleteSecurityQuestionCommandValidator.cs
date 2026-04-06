using FluentValidation;

namespace LinkPara.Identity.Application.Features.SecurityQuestions.Commands.DeleteSecurityQuestion;

public class DeleteSecurityQuestionCommandValidator : AbstractValidator<DeleteSecurityQuestionCommand>
{
    public DeleteSecurityQuestionCommandValidator()
    {
        RuleFor(u => u.Id)
            .NotNull().NotEmpty();
    }
}