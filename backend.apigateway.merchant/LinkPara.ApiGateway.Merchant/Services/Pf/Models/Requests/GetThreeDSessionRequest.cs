using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetThreeDSessionRequest : RequestHeaderInfo
{
    public decimal Amount { get; set; }
    public decimal PointAmount { get; set; }
    public string CardToken { get; set; }
    public string Currency { get; set; }
    public PaymentType PaymentType { get; set; }
    public int InstallmentCount { get; set; }
    public string LanguageCode { get; set; }
    public Guid? SubMerchantId { get; set; }
    public bool? IsInsurancePayment { get; set; }
    public bool? IsTopUpPayment { get; set; }
}