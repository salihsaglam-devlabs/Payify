using FluentValidation;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Queries.GetPricingProfileById;

public class GetPricingProfileByIdQueryValidator : AbstractValidator<GetPricingProfileByIdQuery>
{
    public GetPricingProfileByIdQueryValidator()
    {
        RuleFor(s => s.Id)
            .NotNull()
            .NotEmpty();
    }
}
