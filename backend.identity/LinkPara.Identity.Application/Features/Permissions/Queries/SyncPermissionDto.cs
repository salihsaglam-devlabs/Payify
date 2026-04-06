using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.Identity.Application.Features.Users.Queries;

public class SyncPermissionDto : IMapFrom<Permission>
{
    public string ClaimType { get; set; }
    public string ClaimValue { get; set; }
    public string Module { get; set; }
    public PermissionOperationType OperationType { get; set; }
    public string NormalizedClaimValue { get; set; }
    public string Description { get; set; }
    public string DisplayName { get; set; }
    public bool Display { get; set; }
}