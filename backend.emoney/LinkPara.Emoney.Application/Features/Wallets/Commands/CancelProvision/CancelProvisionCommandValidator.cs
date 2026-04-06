using FluentValidation;

namespace LinkPara.Emoney.Application.Features.Wallets.Commands.CancelProvision;

public class CancelProvisionCommandValidator : AbstractValidator<CancelProvisionCommand>
{
    public CancelProvisionCommandValidator()
    {
        RuleFor(c => c.ConversationId).NotEmpty().NotNull();
    }
}