using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.SharedModels.Banking.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Emoney.Application.Features.PricingProfiles.Queries.GetCardTopupCommission;

public class GetCardTopupCommissionQuery : SearchQueryParams, IRequest<PricingProfileItemDto>
{
    public TransferType? TransferType { get; set; }
}

public class GetCardTopupCommissionQueryHandler : IRequestHandler<GetCardTopupCommissionQuery, PricingProfileItemDto>
{
    private readonly IPricingProfileService _service;

    public GetCardTopupCommissionQueryHandler(IPricingProfileService service)
    {
        _service = service;
    }

    public async Task<PricingProfileItemDto> Handle(GetCardTopupCommissionQuery request, CancellationToken cancellationToken)
    {
        return await _service.GetCardTopupCommissionAsync();
    }
}
