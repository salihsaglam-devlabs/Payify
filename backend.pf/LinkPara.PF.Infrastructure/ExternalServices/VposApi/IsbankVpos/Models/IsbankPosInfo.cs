namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.IsbankVpos.Models;

public class IsbankPosInfo : IPosInfo
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string MerchantNumber { get; set; }
    public string MerchantPassword { get; set; }
    public string TerminalNumber { get; set; }
    public string BaseUrl { get; set; }
    public string TokenUrl { get; set; }
    public string NonSecureUrl { get; set; }
    public string ThreeDSecureUrl { get; set; }
}