using LinkPara.ApiGateway.Services.DigitalKyc.Models.Enums;

namespace LinkPara.ApiGateway.Services.DigitalKyc.Models.Requests.ScSoft;
public class CheckHeadPoseRequest
{
    public string SessionId { get; set; }
    public string Image { get; set; }
    public string IdentityNumber { get; set; }
    public string Direction { get; set; }
    public ScSoftProcessType ProcessType { get; set; }
}
