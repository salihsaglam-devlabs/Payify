namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class GetThreeDSessionResultRequest : RequestHeaderInfo
{
    public string ThreeDSessionId { get; set; }
    public string LanguageCode { get; set; }
}
