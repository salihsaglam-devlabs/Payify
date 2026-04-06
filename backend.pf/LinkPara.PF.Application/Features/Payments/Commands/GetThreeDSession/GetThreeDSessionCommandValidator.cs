using FluentValidation;

namespace LinkPara.PF.Application.Features.Payments.Commands.GetThreeDSession;

public class GetThreeDSessionCommandValidator : AbstractValidator<GetThreeDSessionCommand>
{
    public GetThreeDSessionCommandValidator()
    {
        RuleFor(s => s.Amount)
            .NotEmpty()
            .NotNull()
            .GreaterThan(0m);

        RuleFor(s => s.CardToken)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.MerchantId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.Currency)
            .NotNull()
            .NotEmpty()
            .MaximumLength(3);

        RuleFor(s => s.PaymentType)
            .IsInEnum();

        RuleFor(s => s.ConversationId)
            .NotNull()
            .NotEmpty();

        RuleFor(s => s.LanguageCode)
            .MaximumLength(2);

        RuleFor(s => s.InstallmentCount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(36);
    }
}
