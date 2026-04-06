
using FluentValidation;
using LinkPara.Identity.Application.Features.UserQuestions.Queries.GetUserQuestion;

namespace LinkPara.Identity.Application.Features.SecurityQuestions.Queries.GetSecurityQuestionById
{
    public class GetSecurityQuestionByIdQueryValidator : AbstractValidator<GetSecurityQuestionByIdQuery>
    {
        public GetSecurityQuestionByIdQueryValidator()
        {
            RuleFor(u => u.Id)
                .NotNull().NotEmpty();
        }
    }
}