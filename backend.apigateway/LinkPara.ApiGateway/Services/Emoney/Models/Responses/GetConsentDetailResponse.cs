using LinkPara.ApiGateway.Services.Emoney.Models.Enums;
using LinkPara.ApiGateway.Services.Emoney.Models.Responses;

namespace LinkPara.ApiGateway.OpenBanking.Services.Emoney.Responses;

public class GetConsentDetailResponse
{
    public ConsentType ConsentTypeValue { get; set; }
    public string RedirectUrl { get; set;}
    public bool IsAccountSelected { get; set;}
    public string PermissionTypes { get; set;}
    public string TppName { get; set;}
    public DateTime LastAccessDate { get; set;}
    public string ReceiverTitle { get; set;}
    public string ReceiverIban { get; set;}
    public string SenderTitle { get; set;}
    public string SenderIban { get; set;}
    public string SenderAccRef { get; set;}
    public string Amount { get; set;}
    public string PaymentIntent { get; set;}
    public string PaymentReferenceInfo { get; set;}
    public string CustomerId { get; set;}
    public string IdentityType { get; set;}
    public string IdentityValue { get; set; }
    public string CorporateIdentityType { get; set; }
    public string CorporateIdentityValue { get; set; }
    public WalletType CustomerTypeValue { get; set; }
    public string TranCurrency { get; set; }
    public string PaymentResource { get; set; }
    public string ConsentTypeDesc { get; set; }
    public string ConsentStatus { get; set; }
    public string CancelDetailCode { get; set; }
    public string PaymentDescription { get; set; }
    public string AuthType { get; set; }
    public string DecoupledIdType { get; set; }
    public string DecoupledIdValue { get; set; }

}
