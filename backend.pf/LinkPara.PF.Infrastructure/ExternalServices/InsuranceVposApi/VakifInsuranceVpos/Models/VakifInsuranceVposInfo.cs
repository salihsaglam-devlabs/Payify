using LinkPara.PF.Infrastructure.ExternalServices.VposApi;

namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Models;

public class VakifInsuranceVposInfo : IPosInfo
{
    public string MerchantId { get; set; }
    public string Password { get; set; }
    public string TerminalNo { get; set; }
    public string ThreeDSecureUrl { get; set; }
    public string EnrollmentUrl { get; set; }
    public string SearchUrl { get; set; }
}