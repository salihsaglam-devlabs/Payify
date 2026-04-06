namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Models;

public class AkbankPosInfo : IPosInfo
{
    public string MerchantSafeId { get; set; }
    public string TerminalSafeId { get; set; }
    public string ThreeDSecureUrl { get; set; }
    public string NonSecureUrl { get; set; }
    public string SecretKey { get; set; }
}
