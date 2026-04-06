using LinkPara.PF.Domain.Enums;

namespace LinkPara.PF.Application.Commons.Models.VposModels.Request;

public class PosPayment3DModelRequest : PosRequestBase
{
    public string Cavv { get; set; }
    public string Eci { get; set; }
    public string PayerTxnId { get; set; }
    public string MD { get; set; }
    public decimal Amount { get; set; }    
    public decimal BonusAmount { get; set; }
    public int? Installment { get; set; }
    public string OrderNumber { get; set; }
    public string OrgOrderNumber { get; set; }
    public int Currency { get; set; }
    public string CurrencyCode { get; set; }
    public string CardNumber { get; set; }
    public string ExpireMonth { get; set; }
    public string ExpireYear { get; set; }
    public string CardHolderName { get; set; }
    public string CardHolderIdentityNumber { get; set; }
    public string SubMerchantCode { get; set; }
    public string SubMerchantName { get; set; }
    public string SubMerchantId { get; set; }
    public string SubMerchantPostalCode { get; set; }
    public string SubMerchantCity { get; set; }
    public string SubMerchantCountry { get; set; }
    public string SubMerchantMcc { get; set; }
    public string SubMerchantGlobalMerchantId { get; set; }
    public string SubMerchantUrl { get; set; }
    public string SubMerchantTerminalNo { get; set; }
    public string SubMerchantTaxNumber { get; set; }
    public string ServiceProviderPspMerchantId { get; set; }
    public VposAuthType AuthType { get; set; }
    public bool IsBlockaged { get; set; }
    public string BankPacket { get; set; }
    public int? BlockageCode { get; set; }
}
