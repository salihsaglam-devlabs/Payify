using FluentValidation;

namespace LinkPara.Identity.Application.Features.Account.Commands.RegisterWithCustomer;

public class RegisterWithCustomerCommandValidator : AbstractValidator<RegisterWithCustomerCommand>
{
    public RegisterWithCustomerCommandValidator()
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
            .MaximumLength(10).WithMessage("Invalid PhoneNumber format");

        RuleFor(x => x.PhoneCode!.Trim())
            .NotNull().NotEmpty()
            .MaximumLength(4).WithMessage("Invalid PhoneCode format");

        RuleFor(x => x.Email)
            .NotNull().NotEmpty()
            .MaximumLength(256);

        RuleFor(x => x.ExternalPersonId)
            .NotNull().NotEmpty()
            .MaximumLength(400);

        RuleFor(x => x.ExternalCustomerId)
            .NotNull().NotEmpty()
            .MaximumLength(400);

    }
}