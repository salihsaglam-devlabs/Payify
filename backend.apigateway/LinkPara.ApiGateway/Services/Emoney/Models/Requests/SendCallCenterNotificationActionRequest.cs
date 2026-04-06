using LinkPara.ApiGateway.Services.Emoney.Models.Enums;

namespace LinkPara.ApiGateway.Services.Emoney.Models.Requests;

public class SendCallCenterNotificationActionRequest
{
    public Guid PushNotificationId { get; set; }
    public CallCenterNotificationStatus Action { get; set; }
}