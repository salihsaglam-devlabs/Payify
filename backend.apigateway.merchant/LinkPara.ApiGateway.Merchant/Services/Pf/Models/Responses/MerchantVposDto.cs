using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

public class MerchantVposDto
{
    public Guid VposId { get; set; }
    public int Priority { get; set; }
    public string SubMerchantCode { get; set; }
    public RecordStatus RecordStatus { get; set; }
}
