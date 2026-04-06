namespace LinkPara.HttpProviders.Emoney.Models;

public class FraudCheckResponse
{
    public Guid FraudId { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}
