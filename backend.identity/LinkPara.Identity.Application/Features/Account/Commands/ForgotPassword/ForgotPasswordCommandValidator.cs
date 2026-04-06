using FluentValidation;

namespace LinkPara.Identity.Application.Features.Account.Commands.ForgotPassword;

public class ForgotPasswordCommandValidator: AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.NewPassword)
            .NotNull().NotEmpty();
        RuleFor(x => x.Token)
            .NotNull().NotEmpty()
            .WithMessage("Invalid token!");
        RuleFor(x => x.UserName)
            .NotNull().NotEmpty().MinimumLength(10).MaximumLength(15)
            .WithMessage("Invalid user name!");
    }
}