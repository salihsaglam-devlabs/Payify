using LinkPara.PF.Application.Commons.Interfaces;
using LinkPara.PF.Application.Features.VirtualPos;
using MediatR;

namespace LinkPara.PF.Application.Features.CostProfiles.Queries.GetCostProfileById;

public class GetCostProfileByIdQuery : IRequest<CostProfilesDto>
{
    public Guid Id { get; set; }
}

public class GetCostProfileByIdQueryHandler : IRequestHandler<GetCostProfileByIdQuery, CostProfilesDto>
{
    private readonly ICostProfileService _costProfileService;

    public GetCostProfileByIdQueryHandler(ICostProfileService costProfileService)
    {
        _costProfileService = costProfileService;
    }
    public async Task<CostProfilesDto> Handle(GetCostProfileByIdQuery request, CancellationToken cancellationToken)
    {
        return await _costProfileService.GetByIdAsync(request);
    }
}
