using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Commons.Models.SubMerchants;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.SubMerchantLimits.Queries.GetAllSubMerchantLimits;

public class GetAllSubMerchantLimitsQuery : SearchQueryParams, IRequest<PaginatedList<SubMerchantLimitDto>>
{
    public Guid? Id { get; set; }
    public TransactionLimitType? TransactionLimitType { get; set; }
    public Period? Period { get; set; }
    public LimitType? LimitType { get; set; }
    public int? MaxPiece { get; set; }
    public decimal? MaxAmount { get; set; }
    public string Currency { get; set; }
    public Guid? SubMerchantId { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class GetAllSubMerchantLimitsQueryHandler : IRequestHandler<GetAllSubMerchantLimitsQuery, PaginatedList<SubMerchantLimitDto>>
{
    private readonly ISubMerchantLimitService _subMerchantLimitService;

    public GetAllSubMerchantLimitsQueryHandler(ISubMerchantLimitService subMerchantLimitService)
    {
        _subMerchantLimitService = subMerchantLimitService;
    }

    public async Task<PaginatedList<SubMerchantLimitDto>> Handle(GetAllSubMerchantLimitsQuery request, CancellationToken cancellationToken)
    {
        return await _subMerchantLimitService.GetListAsync(request);
    }
}