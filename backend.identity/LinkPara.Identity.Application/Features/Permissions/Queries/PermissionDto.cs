using LinkPara.Identity.Application.Common.Mappings;
using LinkPara.Identity.Domain.Entities;
using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.Identity.Application.Features.Users.Queries;

public class PermissionDto : IMapFrom<Permission>
{
    public string Module { get; set; }
    public string DisplayName { get; set; }
    public List<ClaimByOperationType> ClaimByOperationTypes { get; set; }
}

public class ClaimByOperationType
{
    public Guid Id { get; set; }
    public PermissionOperationType OperationType { get; set; }
    public string ClaimValue { get; set; }
    public string Description { get; set; }
}