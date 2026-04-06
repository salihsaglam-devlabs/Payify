using LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.Models.Enums;

namespace LinkPara.ApiGateway.BackOffice.Services.DigitalKyc.Models;
public class CustomerKycProcessResponse
{
    public string SessionId { get; set; }
    public string Content { get; set; }
    public ScSoftProcessType ProcessType { get; set; }
    public KycSessionState KycStepState { get; set; }
    public string Description { get; set; }
}
