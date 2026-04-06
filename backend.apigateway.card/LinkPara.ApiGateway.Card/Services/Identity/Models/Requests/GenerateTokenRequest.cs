namespace LinkPara.ApiGateway.Card.Services.Identity.Models.Requests;

public class GenerateTokenRequest
{
    public string ExternalPersonId { get; set; }
    public string ExternalCustomerId { get; set; }
}
