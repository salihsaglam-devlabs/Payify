namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class CardTokenResponse : ResponseModel
{
    public string CardToken { get; set; }
    public Guid CardTopupRequestId { get; set; }
}
