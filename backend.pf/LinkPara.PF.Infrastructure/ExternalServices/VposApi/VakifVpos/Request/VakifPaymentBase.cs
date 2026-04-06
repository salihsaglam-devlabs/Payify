namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Request;

public class VakifPaymentBase : VakifRequestBase
{
    public string TransactionType { get; set; }
    public string CurrencyAmount { get; set; }
    public string MerchantType { get; set; }
    public string TransactionId { get; set; }
    public string HostSubMerchantId { get; set; }
    public int NumberOfInstallments { get; set; }
    public int CurrencyCode { get; set; }
    public string Pan { get; set; }
    public string Cvv { get; set; }
    public string Identity { get; set; }
    public string Expiry { get; set; }
    public string ClientIp { get; set; }
    public string TransactionDeviceSource { get; set; }    
}