using FluentValidation;

namespace LinkPara.Identity.Application.Features.Auth.Commands.UserLogin
{
    public class UserLoginCommandValidator : AbstractValidator<UserLoginCommand>
    {
        public UserLoginCommandValidator()
        {
            RuleFor(x => x.Password)
                .NotNull().MinimumLength(6).MaximumLength(6)
                .WithMessage("Invalid password.");
        }
    }
}
