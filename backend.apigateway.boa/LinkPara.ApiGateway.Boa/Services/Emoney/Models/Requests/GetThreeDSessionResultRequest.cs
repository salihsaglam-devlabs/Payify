namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Requests;

public class GetThreeDSessionResultRequest
{
    public string ThreeDSessionId { get; set; }
    public string LanguageCode = "TR";
    public Guid CardTopupRequestId { get; set; }
}
