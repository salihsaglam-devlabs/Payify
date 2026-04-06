using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class MerchantResponseCode : AuditEntity
{
    public string ResponseCode { get; set; }
    public string Description { get; set; }
    public string DisplayMessageTr { get; set; }
    public string DisplayMessageEn { get; set; }
}
