namespace LinkPara.HttpProviders.Emoney.Models;

public class ResumeRequest
{
    public string CommandName { get; set; }
    public string Data { get; set; }
    public Guid FraudId { get; set; }
}
