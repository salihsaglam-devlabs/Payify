using FluentValidation;

namespace LinkPara.Identity.Application.Features.Account.Commands.UpdatePhoneNumber;
public class UpdatePhoneNumberCommandValidator : AbstractValidator<UpdatePhoneNumberCommand>
{
    public UpdatePhoneNumberCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.NewPhoneNumber!.Trim())
            .NotNull()
            .NotEmpty()
            .MaximumLength(20)
            .WithMessage("Invalid PhoneNumber format");

        RuleFor(x => x.NewPhoneCode!.Trim())
            .NotNull()
            .NotEmpty()
            .MaximumLength(4)
            .WithMessage("Invalid PhoneCode format");

        RuleFor(x => x.Token)
            .NotNull()
            .NotEmpty();
    }
}
