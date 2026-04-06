using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;
using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.PF.Application.Features.MerchantHistory.Queries.GetFilterMerchantHistory;

public class GetFilterMerchantHistoryQuery : SearchQueryParams, IRequest<PaginatedList<MerchantHistoryDto>>
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public PermissionOperationType? PermissionOperationType { get; set; }
    public string MerchantName { get; set; }
    public string CreatedNameBy { get; set; }
}

public class GetFilterMerchantHistoryQueryHandler : IRequestHandler<GetFilterMerchantHistoryQuery, PaginatedList<MerchantHistoryDto>>
{
    private readonly IMerchantHistoryService _merchantHistoryService;

    public GetFilterMerchantHistoryQueryHandler(IMerchantHistoryService merchantHistoryService)
    {
        _merchantHistoryService = merchantHistoryService;
    }
    public async Task<PaginatedList<MerchantHistoryDto>> Handle(GetFilterMerchantHistoryQuery request, CancellationToken cancellationToken)
    {
        return await _merchantHistoryService.GetFilterListAsync(request);
    }
}
