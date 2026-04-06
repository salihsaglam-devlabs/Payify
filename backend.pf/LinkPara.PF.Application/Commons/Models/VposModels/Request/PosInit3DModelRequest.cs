using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.VposModels.Request;

public class PosInit3DModelRequest : PosRequestBase
{
    public string CardNumber { get; set; }
    public string Cvv2 { get; set; }
    public string ExpireMonth { get; set; }
    public string ExpireYear { get; set; }
    public decimal Amount { get; set; }
    public int Installment { get; set; }
    public string OkUrl { get; set; }
    public string FailUrl { get; set; }
    public string OrderNumber { get; set; }
    public string SubMerchantCode { get; set; }
    public string SubMerchantName { get; set; }
    public string SubMerchantId { get; set; }
    public string SubMerchantPostalCode { get; set; }
    public string SubMerchantCity { get; set; }
    public string SubMerchantCountry { get; set; }
    public string SubMerchantMcc { get; set; }
    public string SubMerchantGlobalMerchantId { get; set; }
    public string SubMerchantUrl { get; set; }
    public string SubMerchantTaxNumber { get; set; }
    public string SubmerchantAddress { get; set; }
    public string SubmerchantDistrict { get; set; }
    public string SubmerchantEmail { get; set; }
    public string SubmerchantPhoneCode { get; set; }
    public string SubmerchantPhoneNumber { get; set; }
    public string ServiceProviderPspMerchantId { get; set; }
    public string CardHolderName { get; set; }
    public CardBrand CardBrand { get; set; }
    public decimal? BonusAmount { get; set; }
    public int Currency { get; set; }
    public string CurrencyCode { get; set; }
    public VposAuthType AuthType { get; set; }
    public Guid ThreedSessionId { get; set; }
    public string VposCallbackUrl { get; set; }
    public bool IsBlockaged { get; set; }
    public int? BlockageCode { get; set; }
}
