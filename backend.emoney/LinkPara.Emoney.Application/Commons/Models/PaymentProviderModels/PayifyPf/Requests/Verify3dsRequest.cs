namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class Verify3dsRequest
{
    public string OrderId { get; set; }
    public Guid ThreedSessionId { get; set; }
    public Dictionary<string, string> FormCollection { get; set; }
}
