using FluentValidation;

namespace LinkPara.Identity.Application.Features.Users.Commands.PatchUser;

public class PatchUserCommandValidator : AbstractValidator<PatchUserCommand>
{
    public PatchUserCommandValidator()
    {
        RuleFor(u => u.UserId)
            .NotNull().NotEmpty();
    }
}