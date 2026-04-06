using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantIntegrators.Queries.GetAllMerchantIntegrator;

public class GetAllMerchantIntegratorQuery : IRequest<PaginatedList<MerchantIntegratorDto>>
{
    public SearchQueryParams SearchQueryParams { get; set; }
}

public class GetAllMerchantIntegratorQueryHandler : IRequestHandler<GetAllMerchantIntegratorQuery, PaginatedList<MerchantIntegratorDto>>
{
    private readonly IMerchantIntegratorService _merchantIntegratorService;

    public GetAllMerchantIntegratorQueryHandler(IMerchantIntegratorService merchantIntegratorService)
    {
        _merchantIntegratorService = merchantIntegratorService;
    }
    public async Task<PaginatedList<MerchantIntegratorDto>> Handle(GetAllMerchantIntegratorQuery request, CancellationToken cancellationToken)
    {
        return await _merchantIntegratorService.GetListAsync(request.SearchQueryParams);
    }
}
