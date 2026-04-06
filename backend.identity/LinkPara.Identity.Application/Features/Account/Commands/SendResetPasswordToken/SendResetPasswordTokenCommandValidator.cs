using FluentValidation;

namespace LinkPara.Identity.Application.Features.Account.Commands.SendResetPasswordToken;
public class SendResetPasswordTokenCommandValidator : AbstractValidator<SendResetPasswordTokenCommand>
{
    public SendResetPasswordTokenCommandValidator()
    {
        RuleFor(x => x.UserName)
                    .NotNull().NotEmpty().MinimumLength(10).MaximumLength(15)
                    .WithMessage("Invalid user name!");
    } 
}
