namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.KuveytVpos.Models;

public class KuveytPosInfo : IPosInfo
{
    public string MerchantId { get; set; }
    public string CustomerId { get; set; }
    public string UserName { get; set; }
    public string Password { get; set; }
    public string NonSecureUrl { get; set; }
    public string ThreeDSecureUrl { get; set; }
    public string EnrollmentUrl { get; set; }
}
