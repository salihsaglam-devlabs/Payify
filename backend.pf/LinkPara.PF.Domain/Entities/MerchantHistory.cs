using LinkPara.SharedModels.Authorization.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantHistory : AuditEntity
{
    public Guid MerchantId { get; set; }
    public PermissionOperationType PermissionOperationType { get; set; }
    public string NewData { get; set; }
    public string OldData { get; set; }
    public string Detail { get; set; }
    public string CreatedNameBy { get; set; }
    public Merchant Merchant { get; set; }
}
