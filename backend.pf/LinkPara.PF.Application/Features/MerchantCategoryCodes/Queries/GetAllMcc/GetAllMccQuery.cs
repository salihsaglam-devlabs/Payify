using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.MerchantCategoryCodes.Queries.GetAllMcc;

public class GetAllMccQuery : SearchQueryParams, IRequest<PaginatedList<MccDto>>
{
    public string Name { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class GetAllMccQueryHandler : IRequestHandler<GetAllMccQuery, PaginatedList<MccDto>>
{
    private readonly IMccService _mccService;

    public GetAllMccQueryHandler(IMccService mccService)
    {
        _mccService = mccService;
    }
    public async Task<PaginatedList<MccDto>> Handle(GetAllMccQuery request, CancellationToken cancellationToken)
    {
        return await _mccService.GetListAsync(request);
    }
}