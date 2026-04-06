using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class ProvisionRequest : RequestHeaderInfo
{
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public string CardToken { get; set; }
    public string Currency { get; set; }
    public PaymentType PaymentType { get; set; }
    public IntegrationMode IntegrationMode { get; set; }
    public Guid? SubMerchantId { get; set; }
    public int InstallmentCount { get; set; }
    public string ThreeDSessionId { get; set; }
    public string OriginalOrderId { get; set; }
    public string LanguageCode { get; set; }
    public string MerchantCustomerName { get; set; }
    public string MerchantCustomerPhoneCode { get; set; }
    public string MerchantCustomerPhoneNumber { get; set; }
    public string Description { get; set; }
    public string CardHolderName { get; set; }
    public bool? IsOnUsPayment { get; set; }
    public string CallbackUrl { get; set; }
    public bool? IsInsurancePayment { get; set; }
    public string CardHolderIdentityNumber { get; set; }
}
