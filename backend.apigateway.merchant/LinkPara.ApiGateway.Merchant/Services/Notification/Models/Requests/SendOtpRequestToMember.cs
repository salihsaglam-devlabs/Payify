using LinkPara.ApiGateway.Merchant.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.Merchant.Services.Notification.Models.Requests;

public class SendOtpRequestToMember
{
    public OtpType OtpType { get; set; }
    public OtpAction Action { get; set; }
}