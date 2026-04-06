using LinkPara.PF.Application.Commons.Mappings;
using LinkPara.PF.Domain.Entities;
using LinkPara.SharedModels.Authorization.Enums;

namespace LinkPara.PF.Application.Commons.Models.Merchants;

public class MerchantHistoryDto : IMapFrom<MerchantHistory>
{
    public Guid MerchantId { get; set; }
    public PermissionOperationType PermissionOperationType { get; set; }
    public string NewData { get; set; }
    public string OldData { get; set; }
    public string Detail { get; set; }
    public string CreatedNameBy { get; set; }
}
