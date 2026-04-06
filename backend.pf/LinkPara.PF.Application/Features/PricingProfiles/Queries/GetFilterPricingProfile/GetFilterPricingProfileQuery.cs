using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.PricingProfiles.Queries.GetFilterPricingProfile;

public class GetFilterPricingProfileQuery : SearchQueryParams, IRequest<PaginatedList<PricingProfileDto>>
{
    public ProfileStatus? ProfileStatus { get; set; }
    public ProfileType? ProfileType { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public string PricingProfileNumber { get; set; }
    public bool? IsPaymentToMainMerchant { get; set; }
}

public class GetFilterPricingProfileQueryHandler : IRequestHandler<GetFilterPricingProfileQuery, PaginatedList<PricingProfileDto>>
{
    private readonly IPricingProfileService _pricingProfileService;

    public GetFilterPricingProfileQueryHandler(IPricingProfileService pricingProfileService)
    {
        _pricingProfileService = pricingProfileService;
    }
    public async Task<PaginatedList<PricingProfileDto>> Handle(GetFilterPricingProfileQuery request, CancellationToken cancellationToken)
    {
        return await _pricingProfileService.GetFilterListAsync(request);

    }
}
