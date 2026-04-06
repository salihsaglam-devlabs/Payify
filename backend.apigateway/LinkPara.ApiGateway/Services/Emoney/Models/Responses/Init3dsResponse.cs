namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class Init3dsResponse : ResponseModel
{
    public string HtmlContent { get; set; }
    public Guid CardTopupRequestId { get; set; }
    public string ThreedSessionId { get; set; }
    public string CallBackUrl { get; set; }
}

