using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;

public class ApiResponseCode : AuditEntity
{
    public string ResponseCode { get; set; }
    public string Description { get; set; }
    public Guid? MerchantResponseCodeId { get; set; }
    public MerchantResponseCode MerchantResponseCode { get; set; }
}
