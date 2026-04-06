using FluentValidation;

namespace LinkPara.Identity.Application.Features.UserQuestions.Queries.GetUserQuestion;

public class GetUserQuestionQueryValidator : AbstractValidator<GetUserQuestionQuery>
{
    public GetUserQuestionQueryValidator()
    {
        RuleFor(u => u.UserId)
            .NotNull().NotEmpty();
    }
}