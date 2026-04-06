namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests;

public class IntegrationGetRequest
{
    public List<string> Types { get; set; } = new();
    public string Reference { get; set; }
    public string IdentityType { get; set; }
    public string IdentityNo { get; set; }
}