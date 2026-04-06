using FluentValidation;
using LinkPara.PF.Application.Features.PricingProfiles.Command.UpdatePricingProfile;

namespace LinkPara.PF.Application.Features.PricingProfiles.Command.UpdatePreviewPricingProfile;

public class UpdatePreviewPricingProfileCommandValidator : AbstractValidator<UpdatePreviewPricingProfileCommand>
{
    public UpdatePreviewPricingProfileCommandValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();

        RuleForEach(x => x.PricingProfileItems)
            .SetValidator(new UpdatePricingProfileItemCommandValidator());
    }
}