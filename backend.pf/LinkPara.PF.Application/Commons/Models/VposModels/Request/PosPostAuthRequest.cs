using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.VposModels.Request;

public class PosPostAuthRequest : PosRequestBase
{
    public string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrgOrderNumber { get; set; }
    public decimal Amount { get; set; }
    public decimal PreAuthAmount { get; set; }
    public int Currency  { get; set; }
    public string CurrencyCode { get; set; }
    public CardBrand CardBrand { get; set; }
    public string SubMerchantCode { get; set; }
    public string SubMerchantName { get; set; }
    public string SubMerchantId { get; set; }
    public string SubMerchantPostalCode { get; set; }
    public string SubMerchantCity { get; set; }
    public string SubMerchantDistrict { get; set; }
    public string SubMerchantAddress { get; set; }
    public string SubMerchantCountry { get; set; }
    public string SubMerchantMcc { get; set; }
    public string SubMerchantGlobalMerchantId { get; set; }
    public string SubMerchantUrl { get; set; }
    public string SubMerchantTaxNumber { get; set; }
    public string ServiceProviderPspMerchantId { get; set; }
    public string Stan { get; set; }
    public string RRN { get; set; }
    public string ProvisionNumber { get; set; }
    public string BankOrderId { get; set; }
    public int? Installment { get; set; }
    public bool IsBlockaged { get; set; }
    public int? BlockageCode { get; set; }
}
