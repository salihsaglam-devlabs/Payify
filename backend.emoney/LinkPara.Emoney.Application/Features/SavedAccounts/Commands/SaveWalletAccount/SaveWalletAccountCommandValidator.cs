using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Commands.SaveWalletAccount;

internal class SaveWalletAccountCommandValidator : AbstractValidator<SaveWalletAccountCommand>
{
    public SaveWalletAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.WalletNumber)
          .NotNull()
          .NotEmpty()
          .MaximumLength(10);

        RuleFor(x => x.Tag)
           .NotEmpty()
           .NotNull()
           .MinimumLength(3)
           .MaximumLength(20);
    }

}
