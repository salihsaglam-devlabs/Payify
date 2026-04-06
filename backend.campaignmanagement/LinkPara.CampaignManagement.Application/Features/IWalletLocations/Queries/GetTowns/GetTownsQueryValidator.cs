using FluentValidation;

namespace LinkPara.CampaignManagement.Application.Features.IWalletLocations.Queries.GetTowns;

public class GetTownsQueryValidator : AbstractValidator<GetTownsQuery>
{
    public GetTownsQueryValidator()
    {
        RuleFor(x => x.CityId)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0);
    }
}
