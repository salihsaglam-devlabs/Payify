using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedWalletAccounts;

public class GetSavedWalletAccountsQueryValidator : AbstractValidator<GetSavedWalletAccountsQuery>
{
    public GetSavedWalletAccountsQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
    }
}
