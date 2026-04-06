namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class ApproveSubMerchantRequest
{
    public Guid SubMerchantId { get; set; }
    public bool IsApprove { get; set; }
    public string RejectReason { get; set; }
    public string ParameterValue { get; set; }
}
