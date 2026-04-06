using FluentValidation;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedBankAccounts;

public class GetSavedBankAccountsQueryValidator : AbstractValidator<GetSavedBankAccountsQuery>
{
    public GetSavedBankAccountsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}
