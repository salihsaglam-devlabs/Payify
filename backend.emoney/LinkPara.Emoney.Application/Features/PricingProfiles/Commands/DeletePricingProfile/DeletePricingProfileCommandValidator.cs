using FluentValidation;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Commands.DeletePricingProfile;

public class DeletePricingProfileCommandValidator : AbstractValidator<DeletePricingProfileItemCommand>
{
    public DeletePricingProfileCommandValidator()
    {
        RuleFor(s => s.Id)
              .NotNull()
              .NotEmpty();
    }
}
