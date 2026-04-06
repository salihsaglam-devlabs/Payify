using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class UpdateMerchantVposRequest
{
    public Guid Id { get; set; }
    public Guid VposId { get; set; }
    public int Priority { get; set; }
    public Guid MerchantId { get; set; }
    public string SubMerchantCode { get; set; }
    public RecordStatus RecordStatus { get; set; }
    public string CreatedBy { get; set; }
}
