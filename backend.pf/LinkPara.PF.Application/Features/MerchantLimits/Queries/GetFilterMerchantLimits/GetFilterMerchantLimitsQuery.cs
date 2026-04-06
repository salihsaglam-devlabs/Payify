using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.Merchants;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantLimits.Queries.GetFilterMerchantLimits
{
    public class GetFilterMerchantLimitsQuery : SearchQueryParams, IRequest<PaginatedList<MerchantLimitDto>>
    {
        public Guid MerchantId { get; set; }
    }
    public class GetFilterMerchantLimitsQueryHandler : IRequestHandler<GetFilterMerchantLimitsQuery, PaginatedList<MerchantLimitDto>>
    {
        private readonly IMerchantLimitService _merchantLimitService;

        public GetFilterMerchantLimitsQueryHandler(IMerchantLimitService merchantLimitService)
        {
            _merchantLimitService = merchantLimitService;
        }
        public async Task<PaginatedList<MerchantLimitDto>> Handle(GetFilterMerchantLimitsQuery request, CancellationToken cancellationToken)
        {
            return await _merchantLimitService.GetFilterListAsync(request);
        }
    }
}
