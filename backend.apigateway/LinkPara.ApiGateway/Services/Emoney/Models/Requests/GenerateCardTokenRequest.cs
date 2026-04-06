namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class GenerateCardTokenRequest
{
    public string CardNumber { get; set; }
    public string ExpireMonth { get; set; }
    public string ExpireYear { get; set; }
    public string Cvv { get; set; }
}