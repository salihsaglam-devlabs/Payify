using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CorporateWallets.Commands.AddUser;

public class AddCorporateWalletUserCommandValidator : AbstractValidator<AddCorporateWalletUserCommand>
{
    public AddCorporateWalletUserCommandValidator()
    {
        RuleFor(x => x.AccountId)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Firstname)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Lastname)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.PhoneCode)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.PhoneNumber)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.BirthDate)
            .NotNull()
            .NotEmpty();
        RuleFor(x => x.Roles)
            .NotNull()
            .NotEmpty();
    }
}