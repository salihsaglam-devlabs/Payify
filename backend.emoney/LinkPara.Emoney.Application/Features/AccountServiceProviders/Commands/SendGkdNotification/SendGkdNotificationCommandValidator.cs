using FluentValidation;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.SendGkdNotification;

public class SendGkdNotificationCommandValidator : AbstractValidator<SendGkdNotificationCommand>
{
    public SendGkdNotificationCommandValidator()
    {
        RuleFor(x => x.CustomerType).NotEmpty().NotNull();
        RuleFor(x => x.CorporateIdentityType).NotEmpty().NotNull().When(x => x.CustomerType == 1);
        RuleFor(x => x.CorporateIdentityValue).NotEmpty().NotNull().When(x => x.CustomerType == 1);
        RuleFor(x => x.DecoupledIdType).NotEmpty().NotNull();
        RuleFor(x => x.DecoupledIdValue).NotEmpty().NotNull();
        RuleFor(x => x.MessageContentTR).NotEmpty().NotNull();
        RuleFor(x => x.MessageContentEN).NotEmpty().NotNull();
        RuleFor(x => x.DeepLink).NotEmpty().NotNull();
    }
}
