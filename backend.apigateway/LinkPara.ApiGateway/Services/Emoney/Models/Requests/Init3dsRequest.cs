namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class Init3dsRequest
{
    public string ThreeDSessionId { get; set; }
    public string LanguageCode = "TR";
    public string CardHolderName { get; set; }
    public Guid CardTopupRequestId { get; set; }
}
