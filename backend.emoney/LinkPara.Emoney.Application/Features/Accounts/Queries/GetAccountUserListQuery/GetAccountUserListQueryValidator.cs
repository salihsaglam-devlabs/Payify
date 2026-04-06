using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Accounts.Queries.GetAccountUserListQuery;

public class GetAccountUserListQueryValidator : AbstractValidator<GetAccountUserListQuery>
{
    public GetAccountUserListQueryValidator()
    {
        RuleFor(s => s.AccountId)
            .NotNull()
            .NotEmpty();
    }
}
