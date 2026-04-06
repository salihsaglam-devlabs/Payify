using FluentValidation;

namespace LinkPara.PF.Application.Features.PricingProfiles.Queries.GetPricingProfileById;

public class GetPricingProfileByIdQueryValidator : AbstractValidator<GetPricingProfileByIdQuery>
{
    public GetPricingProfileByIdQueryValidator()
    {
        RuleFor(s => s.Id).NotNull().NotEmpty();
    }
}
