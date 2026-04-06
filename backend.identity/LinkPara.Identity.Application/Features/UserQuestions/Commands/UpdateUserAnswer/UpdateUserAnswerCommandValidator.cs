using FluentValidation;

namespace LinkPara.Identity.Application.Features.UserQuestions.Commands.UpdateUserAnswer;

public class UpdateUserAnswerCommandValidator : AbstractValidator<UpdateUserAnswerCommand>
{
    public UpdateUserAnswerCommandValidator()
    {
        RuleFor(u => u.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(u => u.SecurityQuestionId)
            .NotNull()
            .NotEmpty();

        RuleFor(u => u.Answer)
            .NotNull()
            .NotEmpty()
            .MaximumLength(400);
    }
}