using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class UpdateSubMerchantRequest
{
    public Guid Id { get; set; }
    public bool IsManuelPaymentPageAllowed { get; set; }
    public bool IsLinkPaymentPageAllowed { get; set; }
    public bool IsOnUsPaymentPageAllowed { get; set; }
    public bool PreAuthorizationAllowed { get; set; }
    public bool PaymentReverseAllowed { get; set; }
    public bool PaymentReturnAllowed { get; set; }
    public bool InstallmentAllowed { get; set; }
    public bool Is3dRequired { get; set; }
    public bool IsExcessReturnAllowed { get; set; }
    public bool InternationalCardAllowed { get; set; }
}
