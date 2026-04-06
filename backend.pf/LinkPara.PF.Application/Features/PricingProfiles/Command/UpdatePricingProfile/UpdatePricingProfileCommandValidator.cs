using FluentValidation;

namespace LinkPara.PF.Application.Features.PricingProfiles.Command.UpdatePricingProfile;

public class UpdatePricingProfileCommandValidator : AbstractValidator<UpdatePricingProfileCommand>
{
    public UpdatePricingProfileCommandValidator()
    {
        RuleFor(x => x.Name).MaximumLength(50)
            .WithMessage("Invalid profile name!");

        RuleFor(s => s.ActivationDate)
           .NotNull()
           .NotEmpty()
           .GreaterThan(DateTime.Now)
           .WithMessage("Invalid activation date!");

        RuleForEach(x => x.PricingProfileItems)
          .SetValidator(new UpdatePricingProfileItemCommandValidator());
    }
}

public class UpdatePricingProfileItemCommandValidator : AbstractValidator<PricingProfileItemDto>
{
    public UpdatePricingProfileItemCommandValidator()
    {
        RuleFor(s => s.CommissionRate)
           .GreaterThanOrEqualTo(0);
        
        RuleFor(s => s.ParentMerchantCommissionRate)
           .GreaterThanOrEqualTo(0);

        RuleFor(s => s.BlockedDayNumber)
           .GreaterThanOrEqualTo(0);
    }
}
