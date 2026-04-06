namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class SaveMerchantVposRequest
{
    public Guid VposId { get; set; }
    public int Priority { get; set; }
    public string SubMerchantCode { get; set; }
}
