using FluentValidation;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;

namespace LinkPara.Emoney.Application.Features.Provisions.Commands.Provision;

public class ProvisionCommandValidator : AbstractValidator<ProvisionCommand>
{
    public ProvisionCommandValidator()
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

        RuleFor(c => c.PartnerId)
            .NotNull().When(x => x.ProvisionSource == ProvisionSource.Partner)
            .NotEmpty().When(x => x.ProvisionSource == ProvisionSource.Partner);

    }
}