using FluentValidation;
using LinkPara.Identity.Application.Common.Helpers;

namespace LinkPara.Identity.Application.Features.Account.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {

        RuleFor(x => x.Email).MaximumLength(256)
            .NotNull().NotEmpty()
            .EmailAddress()
            .Must(x => !TurkishCharacterHelper.ContainsTurkishCharacters(x))
            .WithMessage("Invalid Email format");

        RuleFor(x => x.PhoneCode).MaximumLength(4)
            .NotNull().NotEmpty();

        RuleFor(x => x.PhoneNumber).NotNull().NotEmpty()
            .WithMessage("Invalid PhoneNumber format");

        RuleFor(x => x.FirstName)
            .MaximumLength(50).NotNull().NotEmpty();

        RuleFor(x => x.LastName)
            .MaximumLength(50).NotNull().NotEmpty();

        RuleFor(x => x.Password)
            .NotNull().NotEmpty();
        When(m => m.IdentityNumber is not null,
            () => { RuleFor(x => x.IdentityNumber).MaximumLength(20); });
    }
}