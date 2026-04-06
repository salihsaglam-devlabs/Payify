using LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Enums;

namespace LinkPara.ApiGateway.CorporateWallet.Services.Notification.Models.Requests;

public class SendOtpRequestToMember
{
    public OtpType OtpType { get; set; }
    public OtpAction Action { get; set; }
}