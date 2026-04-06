using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantPools.Queries.GetFilterMerchantPool;

public class GetFilterMerchantPoolQuery : SearchQueryParams, IRequest<PaginatedList<MerchantPoolDto>>
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public CompanyType? CompanyType { get; set; }
    public MerchantPoolStatus? MerchantPoolStatus { get; set; }
    public MerchantType? MerchantType { get; set; }
    public PosType? PosType { get; set; }
    public int? MoneyTransferStartHourStart { get; set; }
    public int? MoneyTransferStartHourFinish { get; set; }
    public int? MoneyTransferStartMinuteStart { get; set; }
    public int? MoneyTransferStartMinuteFinish { get; set; }
}

public class GetFilterMerchantPoolQueryHandler : IRequestHandler<GetFilterMerchantPoolQuery, PaginatedList<MerchantPoolDto>>
{
    private readonly IMerchantPoolService _merchantPoolService;

    public GetFilterMerchantPoolQueryHandler(IMerchantPoolService merchantPoolService)
    {
        _merchantPoolService = merchantPoolService;
    }
    public async Task<PaginatedList<MerchantPoolDto>> Handle(GetFilterMerchantPoolQuery request, CancellationToken cancellationToken)
    {
        return await _merchantPoolService.GetFilterListAsync(request);
    }
}
