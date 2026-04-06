using FluentValidation;

namespace LinkPara.PF.Application.Features.PricingProfiles.Command.DeletePricingProfile;

public class DeletePricingProfileCommandValidator : AbstractValidator<DeletePricingProfileCommand>
{
    public DeletePricingProfileCommandValidator()
    {
        RuleFor(x => x.Id).NotNull().NotEmpty();
    }
}
