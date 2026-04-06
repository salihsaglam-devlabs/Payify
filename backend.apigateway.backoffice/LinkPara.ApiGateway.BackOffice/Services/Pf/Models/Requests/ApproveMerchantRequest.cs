using LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Pf.Models.Requests;

public class ApproveMerchantRequest
{
    public Guid MerchantId { get; set; }
    public MerchantStatus MerchantStatus { get; set; }
    public string RejectReason { get; set; }
    public string ParameterValue { get; set; }
}
