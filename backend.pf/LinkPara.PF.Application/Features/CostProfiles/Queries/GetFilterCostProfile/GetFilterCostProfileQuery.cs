using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.PF.Application.Features.CostProfiles.Queries.GetFilterCostProfile;

public class GetFilterCostProfileQuery : SearchQueryParams, IRequest<PaginatedList<CostProfilesDto>>
{
    public int? BankCode { get; set; }
    public PosType? PosType { get; set; }
    public Guid? VposId { get; set; }
    public Guid? PhysicalPosId { get; set; }
    public ProfileStatus? ProfileStatus { get; set; }
    public ProfileSettlementMode? ProfileSettlementMode { get; set; }
    public DateTime? CreateDateStart { get; set; }
    public DateTime? CreateDateEnd { get; set; }
}

public class GetFilterCostProfileQueryHandler : IRequestHandler<GetFilterCostProfileQuery, PaginatedList<CostProfilesDto>>
{
    private readonly ICostProfileService _costProfileService;

    public GetFilterCostProfileQueryHandler(ICostProfileService costProfileService)
    {
        _costProfileService = costProfileService;
    }
    public async Task<PaginatedList<CostProfilesDto>> Handle(GetFilterCostProfileQuery request, CancellationToken cancellationToken)
    {
        return await _costProfileService.GetFilterListAsync(request);
    }
}
