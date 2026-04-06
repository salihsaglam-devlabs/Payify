using FluentValidation;

namespace LinkPara.PF.Application.Features.Payments.Commands.Inquire;

public class InquireCommandValidator : AbstractValidator<InquireCommand>
{
    public InquireCommandValidator()
    {
        RuleFor(s => s.PaymentConversationId)
            .NotNull()
            .NotEmpty()
            .When(s => string.IsNullOrEmpty(s.OrderId));

        RuleFor(s => s.OrderId)
            .NotNull()
            .NotEmpty()
            .When(s => string.IsNullOrEmpty(s.PaymentConversationId));       
        
        RuleFor(s => s.LanguageCode)
            .MaximumLength(2);
        
        RuleFor(s => s.MerchantId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.ConversationId)
            .NotNull()
            .NotEmpty();
    }
}