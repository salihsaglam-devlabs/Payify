using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;
using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Responses;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class SaveSubMerchantRequest
{
    public string Name { get; set; }
    public MerchantType MerchantType { get; set; }
    public Guid MerchantId { get; set; }
    public int City { get; set; }
    public string CityName { get; set; }
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
    public RecordStatus RecordStatus { get; set; }
}
