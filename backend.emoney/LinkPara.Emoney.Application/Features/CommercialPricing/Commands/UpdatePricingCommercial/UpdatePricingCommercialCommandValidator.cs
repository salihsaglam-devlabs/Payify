using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CommercialPricing.Commands.UpdatePricingCommercial;

public class UpdatePricingCommercialCommandValidator : AbstractValidator<UpdateCommercialPricingCommand>
{
    public UpdatePricingCommercialCommandValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
        
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