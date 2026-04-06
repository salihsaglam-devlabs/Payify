namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class ReverseRequest : RequestHeaderInfo
{
    public string OrderId { get; set; }
    public string LanguageCode { get; set; }
}
