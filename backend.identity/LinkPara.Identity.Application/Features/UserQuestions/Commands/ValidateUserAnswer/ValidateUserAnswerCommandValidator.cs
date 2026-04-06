using FluentValidation;

namespace LinkPara.Identity.Application.Features.UserQuestions.Commands.ValidateUserAnswer;

public class ValidateUserAnswerCommandValidator : AbstractValidator<ValidateUserAnswerCommand>
{
    public ValidateUserAnswerCommandValidator()
    {
        RuleFor(u => u.UserId)
            .NotNull().NotEmpty();
        RuleFor(u => u.Answer)
            .NotNull().NotEmpty().MaximumLength(400);
    }
}