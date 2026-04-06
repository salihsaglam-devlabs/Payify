using FluentValidation;

namespace LinkPara.PF.Application.Features.Installments.Queries.CalculateInstallmentPricing;

public class CalculateInstallmentPricingQueryValidator: AbstractValidator<CalculateInstallmentPricingQuery>
{
    public CalculateInstallmentPricingQueryValidator()
    {
        RuleFor(b => b.MerchantNumber)
            .NotNull()
            .NotEmpty();
    }
}