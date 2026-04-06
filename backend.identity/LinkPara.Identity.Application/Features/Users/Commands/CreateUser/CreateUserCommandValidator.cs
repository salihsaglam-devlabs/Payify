using FluentValidation;
using LinkPara.Identity.Domain.Enums;

namespace LinkPara.Identity.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotNull().NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.FirstName)
            .NotNull().NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotNull().NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.PhoneNumber!.Trim()).NotNull().NotEmpty()
            .MaximumLength(10).WithMessage("Invalid PhoneNumber format")
            .When(x => x.UserType != UserType.ApplicationUser);

        RuleFor(x => x.PhoneCode!.Trim())
            .NotNull().NotEmpty()
            .MaximumLength(4).WithMessage("Invalid PhoneCode format");

        RuleFor(x => x.UserType)
            .IsInEnum();

        RuleFor(x => x.Roles)
            .NotNull().NotEmpty().When(x => x.UserType != UserType.ApplicationUser &&
                                            x.UserType != UserType.Representative &&
                                            x.UserType != UserType.Branch &&
                                            x.UserType != UserType.Individual);
    }
}