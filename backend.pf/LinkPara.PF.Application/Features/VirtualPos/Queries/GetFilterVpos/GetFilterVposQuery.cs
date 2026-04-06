using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using LinkPara.SharedModels.Persistence;
using MediatR;

namespace LinkPara.PF.Application.Features.VirtualPos.Queries.GetFilterVpos;

public class GetFilterVposQuery : SearchQueryParams, IRequest<PaginatedList<VposDto>>
{
    public int? BankCode { get; set; }
    public VposStatus? VposStatus { get; set; }
    public VposType? VposType { get; set; }
    public RecordStatus? RecordStatus { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
    public bool? IsInsuranceVpos { get; set; }
    public bool? IsTopUpVpos { get; set; }
    public bool? IsOnUsVpos { get; set; }
}

public class GetFilterVposQueryHandler : IRequestHandler<GetFilterVposQuery, PaginatedList<VposDto>>
{
    private readonly IVposService _vposService;

    public GetFilterVposQueryHandler(IVposService vposService)
    {
        _vposService = vposService;
    }
    public async Task<PaginatedList<VposDto>> Handle(GetFilterVposQuery request, CancellationToken cancellationToken)
    {
        return await _vposService.GetFilterListAsync(request);
    }
}
