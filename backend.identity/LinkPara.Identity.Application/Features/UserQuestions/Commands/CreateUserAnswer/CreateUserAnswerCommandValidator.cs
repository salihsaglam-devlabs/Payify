using FluentValidation;

namespace LinkPara.Identity.Application.Features.UserQuestions.Commands.CreateUserAnswer;

public class CreateUserAnswerCommandValidator : AbstractValidator<CreateUserAnswerCommand>
{
    public CreateUserAnswerCommandValidator()
    {
        RuleFor(u => u.UserId)
            .NotNull().NotEmpty();

        RuleFor(u => u.SecurityQuestionId)
            .NotNull().NotEmpty();

        RuleFor(u => u.Answer)
            .NotNull().MaximumLength(400);
    }
}