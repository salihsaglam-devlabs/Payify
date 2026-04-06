using LinkPara.SharedModels.BusModels.IntegrationEvents.Logging.Enums;

namespace LinkPara.SharedModels.BusModels.IntegrationEvents.Logging;

public class UserLoginLog
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Email { get; set; }
    public string RemoteIpAddress { get; set; }
    public string LocaleIpAddress { get; set; }
    public string UserAgent { get; set; }
    public LoginResult LoginResultCode { get; set; }
    public string LoginResult { get; set; }
    public DateTime CreateDate { get; set; }
}
