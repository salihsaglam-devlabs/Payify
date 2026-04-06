using FluentValidation;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedBankAccountById;

public class GetSavedBankAccountByIdQueryValidator : AbstractValidator<GetSavedBankAccountByIdQuery>
{
    public GetSavedBankAccountByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .NotEmpty();
    }
}
