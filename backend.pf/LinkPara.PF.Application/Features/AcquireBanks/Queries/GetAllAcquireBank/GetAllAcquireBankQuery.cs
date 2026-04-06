using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.AcquireBanks.Queries.GetAllAcquireBank;

public class GetAllAcquireBankQuery : SearchQueryParams, IRequest<PaginatedList<AcquireBankDto>>
{
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public RecordStatus? RecordStatus { get; set; }
}

public class GetAllAcquireBankQueryHandler : IRequestHandler<GetAllAcquireBankQuery, PaginatedList<AcquireBankDto>>
{
    private readonly IAcquireBankService _acquireBankService;

    public GetAllAcquireBankQueryHandler(IAcquireBankService acquireBankService)
    {
        _acquireBankService = acquireBankService;
    }

    public async Task<PaginatedList<AcquireBankDto>> Handle(GetAllAcquireBankQuery request, CancellationToken cancellationToken)
    {
        return await _acquireBankService.GetListAsync(request);
    }
}
