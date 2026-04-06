namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Response;

public class VakifInsurancePaymentNonSecureResponse : VakifInsuranceResponseBase
{
    public string TransactionType { get; set; }
    public string ReferenceTransactionId { get; set; }
    public string TerminalNo { get; set; }
    public decimal TotalPoint { get; set; }
    public decimal CurrencyAmount { get; set; }
    public int CurrencyCode { get; set; }
    public int InstallmentCount { get; set; }
    public string OrderId { get; set; }
    public int ThreeDSecureType { get; set; }
    public int TransactionDeviceSource { get; set; }
    public int BatchNo { get; set; }
    public decimal TlAmount { get; set; }
    public int MerchantType { get; set; }
    public string HostResultCode { get; set; }
}