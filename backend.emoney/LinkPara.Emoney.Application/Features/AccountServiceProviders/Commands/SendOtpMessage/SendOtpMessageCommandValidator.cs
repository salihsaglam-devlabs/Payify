using FluentValidation;

namespace LinkPara.Emoney.Application.Features.AccountServiceProviders.Commands.SendOtpMessage;

public class SendOtpMessageCommandValidator : AbstractValidator<SendOtpMessageCommand>
{
    public SendOtpMessageCommandValidator()
    {
        RuleFor(x => x.SmsContent).NotEmpty().NotNull();
    }
}
