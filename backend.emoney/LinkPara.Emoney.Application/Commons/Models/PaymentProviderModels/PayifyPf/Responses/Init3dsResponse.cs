namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Responses;

public class Init3dsResponse : ResponseModel
{
    public string HtmlContent { get; set; }
    public Guid CardTopupRequestId { get; set; }
    public string ThreedSessionId { get; set; }
    public string CallBackUrl { get; set; }
}
