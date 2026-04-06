using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Queries.GetSavedWalletAccountById;

internal class GetSavedWalletAccountByIdQueryValidator : AbstractValidator<GetSavedWalletAccountByIdQuery>
{
    public GetSavedWalletAccountByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotNull()
            .NotEmpty();
    }
}