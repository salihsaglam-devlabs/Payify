namespace LinkPara.Emoney.Application.Commons.Models.PaymentProviderModels.PayifyPf.Requests;

public class InquireRequestModel : RequestHeaderInfo
{
    public Guid CardTopupRequestId { get; set; }
    public string PaymentConversationId { get; set; }
    public string OrderId { get; set; }
    public string LanguageCode { get; set; } = "TR";
}
