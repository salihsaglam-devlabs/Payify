using LinkPara.SharedModels.Pagination;
using MediatR;
using LinkPara.PF.Application.Commons.Interfaces;


namespace LinkPara.PF.Application.Features.MerchantHistory.Queries.GetAllMerchantHistory
{
    public class GetAllMerchantHistoryQuery : SearchQueryParams, IRequest<PaginatedList<MerchantHistoryDto>>
    {
    }
    public class GetAllMerchantHistoryHandler : IRequestHandler<GetAllMerchantHistoryQuery, PaginatedList<MerchantHistoryDto>>
    {
        private readonly IMerchantHistoryService _merchantHistoryService;

        public GetAllMerchantHistoryHandler(IMerchantHistoryService merchantHistoryService)
        {
            _merchantHistoryService = merchantHistoryService;
        }
        public async Task<PaginatedList<MerchantHistoryDto>> Handle(GetAllMerchantHistoryQuery request, CancellationToken cancellationToken)
        {
            return await _merchantHistoryService.GetAllMerchantHistoryAsync(request);
        }
    }
}