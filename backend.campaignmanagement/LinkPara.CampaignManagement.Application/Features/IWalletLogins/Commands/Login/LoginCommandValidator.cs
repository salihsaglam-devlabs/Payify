using FluentValidation;

namespace LinkPara.CampaignManagement.Application.Features.IWalletLogins.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotNull()
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotNull()
            .NotEmpty();
    }
}
