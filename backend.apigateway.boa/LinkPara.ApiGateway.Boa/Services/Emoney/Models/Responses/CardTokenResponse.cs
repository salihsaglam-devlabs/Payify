namespace LinkPara.ApiGateway.Boa.Services.Emoney.Models.Responses;

public class CardTokenResponse : ResponseModel
{
    public string CardToken { get; set; }
    public Guid CardTopupRequestId { get; set; }
}
