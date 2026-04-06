using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Accounts.Commands.ValidateIdentity;

public class ValidateIdentityCommandValidator : AbstractValidator<ValidateIdentityCommand>
{
    public ValidateIdentityCommandValidator()
    {
        RuleFor(s => s.IdentityNo).NotEmpty().NotNull();
        RuleFor(s => s.DateOfBirth).NotEmpty().NotNull();
        RuleFor(s => s.UserId).NotEmpty().NotNull();
        RuleFor(s => s.FirstName).NotEmpty().NotNull();
        RuleFor(s => s.LastName).NotEmpty().NotNull();
    }
}