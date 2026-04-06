namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class InquireRequest : RequestHeaderInfo
{
    public string PaymentConversationId { get; set; }
    public string OrderId { get; set; }  
    public string LanguageCode { get; set; }
}