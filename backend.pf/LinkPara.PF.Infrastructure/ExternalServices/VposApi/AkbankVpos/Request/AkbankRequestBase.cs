namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Request;

public class AkbankRequestBase
{
    public string MerchantSafeId { get; set; }
    public string TerminalSafeId { get; set; }
    public string Version { get; set; }
    public string TxnCode { get; set; }
    public string RequestDateTime { get; set; }
    public string RandomNumber { get; set; }
    public string SecretKey { get; set; }
    public string Lang { get; set; }
    public string EmailAddress { get; set; }
    public string OrderId { get; set; }
    public string SubMerchantId { get; set; }
    public int CurrencyCode { get; set; }
}
