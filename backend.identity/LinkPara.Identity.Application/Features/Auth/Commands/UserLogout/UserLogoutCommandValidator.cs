using FluentValidation;

namespace LinkPara.Identity.Application.Features.Auth.Commands.UserLogout
{
    public class UserLogoutCommandValidator : AbstractValidator<UserLogoutCommand>
    {
        public UserLogoutCommandValidator()
        {

        }
    }
}
