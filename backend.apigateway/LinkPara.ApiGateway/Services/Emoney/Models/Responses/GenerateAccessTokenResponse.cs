namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class GenerateAccessTokenResponse
{
    public string AccessToken { get; set; }
    public Guid CardTopupRequestId { get; set; }
}
