namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class ReturnRequest : RequestHeaderInfo
{
    public string OrderId { get; set; }
    public string LanguageCode { get; set; }
    public decimal Amount { get; set; }
}
