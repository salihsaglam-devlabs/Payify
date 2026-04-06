using FluentValidation;

namespace LinkPara.Identity.Application.Features.Auth.Commands.GenerateToken;

public class GenerateTokenCommandValidator : AbstractValidator<GenerateTokenCommand>
{
    public GenerateTokenCommandValidator()
    {
        RuleFor(x => x.ExternalCustomerId).NotNull().NotEmpty();
        RuleFor(x => x.ExternalPersonId).NotNull().NotEmpty();
    }
}