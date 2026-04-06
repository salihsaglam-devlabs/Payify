
using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Identity.Models.Responses;

public class PermissionDto
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