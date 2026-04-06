using LinkPara.ApiGateway.Merchant.Services.Pf.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Pf.Models.Requests;

public class SendMerchantNotificationRequest
{
    public string[] To { get; set; }
    public MerchantNotificationType NotificationType { get; set; }
    public string Message { get; set; }
    public string Subject { get; set; }
    public string PreHeader { get; set; }
}