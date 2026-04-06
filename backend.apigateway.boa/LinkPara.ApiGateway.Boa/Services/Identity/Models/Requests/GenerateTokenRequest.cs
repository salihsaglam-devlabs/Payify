namespace LinkPara.ApiGateway.Boa.Services.Identity.Models.Requests;

public class GenerateTokenRequest
{
    public string ExternalPersonId { get; set; }
    public string ExternalCustomerId { get; set; }
}
