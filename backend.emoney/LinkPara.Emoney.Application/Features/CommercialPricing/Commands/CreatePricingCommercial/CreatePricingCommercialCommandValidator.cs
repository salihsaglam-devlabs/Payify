using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CommercialPricing.Commands.CreatePricingCommercial;

public class CreatePricingCommercialCommandValidator : AbstractValidator<CreatePricingCommercialCommand>
{
    public CreatePricingCommercialCommandValidator()
    {
        RuleFor(s => s.MaxDistinctSenderCount)
            .GreaterThanOrEqualTo(0);

        RuleFor(s => s.MaxDistinctSenderCountWithAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(s => s.MaxDistinctSenderAmount)
            .GreaterThanOrEqualTo(0);
        
        RuleFor(s => s.CommissionRate)
            .NotNull()
            .GreaterThanOrEqualTo(0);

        RuleFor(s => s.ActivationDate)
            .NotNull()
            .NotEmpty()
            .GreaterThanOrEqualTo(DateTime.Now);

        RuleFor(s => s.CurrencyCode)
            .NotNull()
            .NotEmpty()
            .MaximumLength(3);

        RuleFor(s => s.PricingCommercialType)
            .NotNull();
    }
}