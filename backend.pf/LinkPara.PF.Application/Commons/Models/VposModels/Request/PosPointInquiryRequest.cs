namespace LinkPara.PF.Application.Commons.Models.VposModels.Request;

public class PosPointInquiryRequest : PosRequestBase
{
    public string CardNumber { get; set; }
    public string Cvv2 { get; set; }
    public string ExpireMonth { get; set; }
    public string ExpireYear { get; set; }
    public string OrderNumber { get; set; }
    public int Currency { get; set; }
    public string SubMerchantCode { get; set; }
    public string SubMerchantTerminalNo { get; set; }
    public string ServiceProviderPspMerchantId { get; set; }
}
