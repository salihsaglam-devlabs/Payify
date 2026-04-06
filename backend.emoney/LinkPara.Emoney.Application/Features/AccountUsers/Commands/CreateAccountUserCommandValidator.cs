using FluentValidation;
using LinkPara.Emoney.Domain.Entities;

namespace LinkPara.Emoney.Application.Features.AccountUsers.Commands;

public class CreateAccountUserCommandValidator : AbstractValidator<AccountUser>
{
    public CreateAccountUserCommandValidator()
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
    }
}
