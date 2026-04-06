using FluentValidation;

namespace LinkPara.Identity.Application.Features.Account.Commands.SendPasswordResetToken;

public class SendPasswordResetTokenCommandValidator: AbstractValidator<SendPasswordResetTokenCommand>
{
    public SendPasswordResetTokenCommandValidator()
    {
        RuleFor(x => x.UserName)
            .NotNull().NotEmpty().MinimumLength(10).MaximumLength(15)
            .WithMessage("Invalid user name!");
    }
}