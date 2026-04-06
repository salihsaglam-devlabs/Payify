using FluentValidation;

namespace LinkPara.Emoney.Application.Features.CommercialPricing.Commands.DeletePricingCommercial;

public class DeletePricingCommercialCommandValidator : AbstractValidator<DeletePricingCommercialCommand>
{
    public DeletePricingCommercialCommandValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
    }
}