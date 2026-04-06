namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class ActiveAccountConsentResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public ActiveAccountConsentDto Result { get; set; }
}