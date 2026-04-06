namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class Init3dsRequest
{
    public string ThreeDSessionId { get; set; }
    public string LanguageCode = "TR";
    public string CardHolderName { get; set; }
    public Guid CardTopupRequestId { get; set; }
}
