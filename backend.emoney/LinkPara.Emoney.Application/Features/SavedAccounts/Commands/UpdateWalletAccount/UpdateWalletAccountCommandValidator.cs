using FluentValidation;

namespace LinkPara.Emoney.Application.Features.SavedAccounts.Commands.UpdateWalletAccount;

public class UpdateWalletAccountCommandValidator : AbstractValidator<UpdateWalletAccountCommand>
{
    public UpdateWalletAccountCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Tag)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100); 
        RuleFor(x => x.WalletNumber)
             .NotNull()
             .NotEmpty()
             .MaximumLength(15);
    }
}