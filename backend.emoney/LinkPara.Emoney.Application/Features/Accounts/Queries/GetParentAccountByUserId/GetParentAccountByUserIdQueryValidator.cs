using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetParentAccountByUserId;

public class GetParentAccountByUserIdQueryValidator : AbstractValidator<GetParentAccountByUserIdQuery>
{
    public GetParentAccountByUserIdQueryValidator()
    {
        RuleFor(s => s.UserId)
            .NotNull()
            .NotEmpty();
    }
}