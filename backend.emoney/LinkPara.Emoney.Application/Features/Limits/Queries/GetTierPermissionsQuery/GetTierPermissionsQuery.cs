using LinkPara.Emoney.Application.Commons.Interfaces;
using LinkPara.Emoney.Domain.Enums;
using LinkPara.HttpProviders.Emoney.Enums;
using MediatR;

namespace LinkPara.Emoney.Application.Features.Limits.Queries.GetTierPermissionsQuery;

public class GetTierPermissionsQuery : IRequest<List<TierPermissionDto>>
{
    public TierLevelType? TierLevel { get; set; }
    public TierPermissionType? PermissionType { get; set; }
    public bool? IsEnabled { get; set; }
}

public class GetTierPermissionsQueryHandler : IRequestHandler<GetTierPermissionsQuery, List<TierPermissionDto>>
{
    private readonly ITierPermissionService _tierPermissionService;

    public GetTierPermissionsQueryHandler(ITierPermissionService tierPermissionService)
    {
        _tierPermissionService = tierPermissionService;
    }

    public async Task<List<TierPermissionDto>> Handle(GetTierPermissionsQuery request, CancellationToken cancellationToken)
    {
        return await _tierPermissionService.GetTierPermissionsQueryAsync(request);
    }
}