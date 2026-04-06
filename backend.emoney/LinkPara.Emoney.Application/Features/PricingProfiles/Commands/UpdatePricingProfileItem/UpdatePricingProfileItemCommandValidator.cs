using FluentValidation;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Commands.UpdatePricingProfileItem;

public class UpdatePricingProfileItemCommandValidator : AbstractValidator<UpdatePricingProfileItemCommand>
{
    public UpdatePricingProfileItemCommandValidator()
    {
        RuleFor(s => s.Fee)
             .GreaterThanOrEqualTo(0);

        RuleFor(s => s.CommissionRate)
            .GreaterThanOrEqualTo(0);

        RuleFor(s => s.MinAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(s => s.MaxAmount)
            .GreaterThanOrEqualTo(0);

        RuleFor(s => s).Must(CheckAmount).WithMessage("MaxAmount must be greater then MinAmount");
    }

    public static bool CheckAmount(UpdatePricingProfileItemCommand model)
    {
        return model.MaxAmount >= model.MinAmount;
    }
}
