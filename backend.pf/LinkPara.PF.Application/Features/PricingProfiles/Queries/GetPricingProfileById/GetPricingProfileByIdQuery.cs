using LinkPara.PF.Application.Commons.Interfaces;
using MediatR;

namespace LinkPara.PF.Application.Features.PricingProfiles.Queries.GetPricingProfileById;

public class GetPricingProfileByIdQuery : IRequest<PricingProfileDto>
{
    public Guid Id { get; set; }
}

public class GetPricingProfileByIdQueryHandler : IRequestHandler<GetPricingProfileByIdQuery, PricingProfileDto>
{
    private readonly IPricingProfileService _pricingProfileService;

    public GetPricingProfileByIdQueryHandler(IPricingProfileService pricingProfileService)
    {
        _pricingProfileService = pricingProfileService;
    }
    public async Task<PricingProfileDto> Handle(GetPricingProfileByIdQuery request, CancellationToken cancellationToken)
    {
        return await _pricingProfileService.GetByIdAsync(request);
    }
}
