using LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.Notification.Models.Requests;

public class SendOtpRequestToMember
{
    public OtpType OtpType { get; set; }
    public OtpAction Action { get; set; }
}