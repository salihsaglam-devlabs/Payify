using FluentValidation;
using LinkPara.PF.Domain.Enums;
using Microsoft.Extensions.Localization;

namespace LinkPara.PF.Application.Features.PricingProfiles.Command.SavePricingProfile;

public class SavePricingProfileCommandValidator : AbstractValidator<SavePricingProfileCommand>
{
    private readonly IStringLocalizer _localizer;
    public SavePricingProfileCommandValidator(IStringLocalizerFactory factory)
    {
        _localizer = factory.Create("Exceptions", "LinkPara.PF.API");

        RuleFor(x => x.Name).MaximumLength(50)
            .WithMessage("Invalid profile name!");

        RuleFor(x => x.ProfileType)
            .IsInEnum();

        RuleFor(s => s.ActivationDate)
           .NotNull()
           .NotEmpty()
           .GreaterThan(DateTime.Now)
           .WithMessage(_localizer.GetString("InvalidActivationDateException").Value);

        RuleForEach(x => x.PricingProfileItems)
           .SetValidator(new SavePricingProfileItemCommandValidator());
        
        RuleFor(x => x)
            .Custom((command, context) =>
            {
                if (command.ProfileType != ProfileType.Standard) return;
                foreach (var item in command.PricingProfileItems.Where(item => item.ParentMerchantCommissionRate != 0))
                {
                    context.AddFailure("ParentMerchantCommissionRate", _localizer.GetString("ParentMerchantCommissionMustBeZeroException").Value);
                }
            });
    }
}

public class SavePricingProfileItemCommandValidator : AbstractValidator<PricingProfileItemDto>
{
    public SavePricingProfileItemCommandValidator()
    {
        RuleFor(s => s.CommissionRate)
           .GreaterThanOrEqualTo(0);
        
        RuleFor(s => s.ParentMerchantCommissionRate)
           .GreaterThanOrEqualTo(0);

        RuleFor(s => s.BlockedDayNumber)
           .GreaterThanOrEqualTo(0);
    }
}
