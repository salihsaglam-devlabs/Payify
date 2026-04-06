using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Provisions.Commands.ReturnProvision;

public class ReturnProvisionCommandValidator : AbstractValidator<ReturnProvisionCommand>
{
    public ReturnProvisionCommandValidator()
    {
        RuleFor(c => c.WalletNumber)
            .MaximumLength(50)
            .NotEmpty()
            .NotNull();

        RuleFor(c => c.CurrencyCode)
            .MaximumLength(10)
            .NotEmpty()
            .NotNull();

        RuleFor(c => c.ConversationId)
            .MaximumLength(100)
            .NotEmpty()
            .NotNull();

        RuleFor(c => c.ClientIpAddress)
            .MaximumLength(50)
            .NotEmpty()
            .NotNull();
    }
}
