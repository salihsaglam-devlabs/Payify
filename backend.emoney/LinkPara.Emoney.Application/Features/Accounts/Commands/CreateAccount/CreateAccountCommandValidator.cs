using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Accounts.Commands.CreateAccount;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(s => s.Firstname)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(s => s.Lastname)
            .NotNull()
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(s => s.Email)
            .NotNull()
            .NotEmpty()
            .MaximumLength(256)
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);

        RuleFor(s => s.PhoneCode)
            .NotNull()
            .NotEmpty()
            .MaximumLength(10);

        RuleFor(s => s.PhoneNumber)
            .NotNull()
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(s => s.AccountType)
            .IsInEnum();

        RuleFor(s => s.AccountKycLevel)
            .IsInEnum();
    }
}
