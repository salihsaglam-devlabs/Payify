namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class GetThreeDSessionResultRequest : RequestHeaderInfo
{
    public string ThreeDSessionId { get; set; }
    public string LanguageCode = "TR";
    public Guid CardTopupRequestId { get; set; }
}
