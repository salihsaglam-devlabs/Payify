using FluentValidation;

namespace LinkPara.Identity.Application.Features.Account.Commands.UpdateEmail;

public class UpdateEmailCommandValidator : AbstractValidator<UpdateEmailCommand>
{
    public UpdateEmailCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull().NotEmpty();

        RuleFor(x => x.NewEmail)
            .NotNull().NotEmpty()
            .MaximumLength(256)
            .EmailAddress(FluentValidation.Validators.EmailValidationMode.AspNetCoreCompatible);

        RuleFor(x => x.Token)
            .NotNull().NotEmpty();
    }
}
