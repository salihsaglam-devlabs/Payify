namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Request;

public class IsbankPaymentRequestBase : IsbankRequestBase
{
    public string CardNumber { get; set; }
    public int CardExpireYear { get; set; }
    public int CardExpireMonth { get; set; }
    public string Cvv { get; set; }
    public decimal Amount { get; set; }
    public int Installment { get; set; }
    public int Currency { get; set; }
    public string Type { get; set; }
    public int TraceNumber { get; set; }
    public string CardHolderName { get; set; }
    public string Hash { get; set; }
    public string Rnd { get; set; }
    public decimal PointAmount { get; set; }
    public int TransactionType { get; set; }
    public string SubMerchantName { get; set; }
    public string SubMerchantId { get; set; }
    public string SubMerchantMcc { get; set; }
    public string SubMerchantCitizenId { get; set; }
    public string SubMerchantCity { get; set; }
    public string SubMerchantPostalCode { get; set; }
    public string SubMerchantUrl { get; set; }
}