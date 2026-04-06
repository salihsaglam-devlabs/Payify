using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class PfPaymentRequest
{
    public string PublicKey { get; set; }
    public string CardToken { get; set; }
    public string OrderNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public string Currency { get; set; }
    public PaymentType PaymentType { get; set; }
    public SecurePaymentType SecurePaymentType { get; set; }
    public int InstallmentCount { get; set; }
    public string MerchantNumber { get; set; }
    public string ThreeDSessionId { get; set; }
    public string MerchantCustomerName { get; set; }
    public string MerchantCustomerPhoneNumber { get; set; }
    public string Description { get; set; }
    public string CardHolderName { get; set; }
}
