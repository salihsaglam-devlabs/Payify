namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Request;

public class NestPayBaseRequest : NestPayBase
{    
    public string OrderId { get; set; }
    public int? NumberOfInstallments { get; set; }
    public string Type { get; set; }
    public string Pan { get; set; }
    public string Expiry { get; set; }
    public string Cvv { get; set; }
    public string Amount { get; set; }
    public decimal BonusAmount { get; set; }
    public int CurrencyCode { get; set; }
    public string SubMerchantName { get; set; }
    public string SubMerchantId { get; set; }
    public string SubMerchantNumber { get; set; }
    public string SubMerchantPostalCode { get; set; }
    public string SubMerchantCity { get; set; }
    public string SubMerchantCountry { get; set; }
    public string SubMerchantMcc { get; set; }
    public string SubMerchantGlobalMerchantId { get; set; }
    public string SubMerchantUrl { get; set; }
    public string SubMerchantTaxNumber { get; set; }
    public string VisaSubmerchantId { get; set; }
    public string VisaPfId { get; set; }
    public string ClientIp { get; set; }
    public bool IsBlockaged { get; set; }
}