namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Models;

public class IvpPosInfo : IPosInfo
{
    public string ShopCode { get; set; }
    public string UserCode { get; set; }
    public string UserPass { get; set; }
    public string NonSecureUrl { get; set; }
    public string ThreeDSecureUrl { get; set; }
    public string MerchantPass { get; set; }
}
