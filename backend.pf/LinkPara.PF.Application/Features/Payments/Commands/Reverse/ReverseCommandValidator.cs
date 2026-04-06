using FluentValidation;

namespace LinkPara.PF.Application.Features.Payments.Commands.Reverse;

public class ReverseCommandValidator : AbstractValidator<ReverseCommand>
{
    public ReverseCommandValidator()
    {
        RuleFor(s => s.MerchantId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.ConversationId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.OrderId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.LanguageCode)
            .MaximumLength(2);
    }
}
