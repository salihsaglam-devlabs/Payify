using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantBlockages.Queries.GetAllMerchantBlockages;

public class GetAllMerchantBlockageQuery : SearchQueryParams, IRequest<PaginatedList<MerchantBlockageDto>>
{
    public MerchantBlockageStatus? MerchantBlockageStatus { get; set; }

    public Guid? MerchantId { get; set; }
}

public class GetAllMerchantBlockageQueryHandler : IRequestHandler<GetAllMerchantBlockageQuery, PaginatedList<MerchantBlockageDto>>
{
    private readonly IMerchantBlockageService _merchantBlockageService;

    public GetAllMerchantBlockageQueryHandler(IMerchantBlockageService merchantBlockageService)
    {
        _merchantBlockageService = merchantBlockageService;
    }
    public async Task<PaginatedList<MerchantBlockageDto>> Handle(GetAllMerchantBlockageQuery request, CancellationToken cancellationToken)
    {
        return await _merchantBlockageService.GetAllAsync(request);
    }
}
