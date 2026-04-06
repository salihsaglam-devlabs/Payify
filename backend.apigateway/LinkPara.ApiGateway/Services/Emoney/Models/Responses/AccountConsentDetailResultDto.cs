namespace LinkPara.ApiGateway.Services.Emoney.Models.Responses;

public class AccountConsentDetailResultDto
{
    public string RequestId { get; set; }
    public string GroupId { get; set; }
    public int StatusCode { get; set; }
    public AccountInfoConsentResultDto Result { get; set; }
}