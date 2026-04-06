using FluentValidation;
using Microsoft.Extensions.Localization;

namespace LinkPara.Identity.Application.Features.Account.Commands.ResetPassword;


public class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{

    private readonly IStringLocalizer _localizer;

    public ResetPasswordCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.Identity.API");

        RuleFor(x => x.NewPassword)
            .NotNull().NotEmpty().MinimumLength(6).MaximumLength(50)
            .WithMessage(_localizer.GetString("PasswordCharactersCountException").Value)
            .NotEqual(q => q.OldPassword)
            .WithMessage(_localizer.GetString("PasswordMustBeDifferentException").Value);

        RuleFor(x => x.OldPassword)
                .NotNull().NotEmpty().MinimumLength(1).MaximumLength(50)
                .WithMessage(_localizer.GetString("InvalidOldPasswordException").Value);

        RuleFor(x => x.UserId)
                .NotEmpty()
                .NotNull()
                .When(x => string.IsNullOrWhiteSpace(x.UserName));
        
        RuleFor(x => x.UserName)
            .NotEmpty()
            .NotNull()
            .When(x => string.IsNullOrWhiteSpace(x.UserId));
    }
}