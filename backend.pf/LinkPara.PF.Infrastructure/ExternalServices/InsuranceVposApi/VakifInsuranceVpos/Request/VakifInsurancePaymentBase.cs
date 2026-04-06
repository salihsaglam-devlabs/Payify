namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Request;

public class VakifInsurancePaymentBase : VakifInsuranceRequestBase
{
    public string TransactionType { get; set; }
    public string CurrencyAmount { get; set; }
    public string TransactionId { get; set; }
    public string InquiryValue { get; set; }
    public string InstallmentCount { get; set; }
    public int CurrencyCode { get; set; }
    public string CardNoFirst { get; set; }
    public string CardNoLast { get; set; }
    public string TransactionDeviceSource { get; set; }    
    public string MerchantType { get; set; } 
}