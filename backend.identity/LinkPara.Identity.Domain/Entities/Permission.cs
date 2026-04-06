using LinkPara.SharedModels.Authorization.Enums;
using LinkPara.SharedModels.Persistence;
using System.ComponentModel.DataAnnotations.Schema;

namespace LinkPara.Identity.Domain.Entities;

public class Permission : AuditEntity, ITrackChange 
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