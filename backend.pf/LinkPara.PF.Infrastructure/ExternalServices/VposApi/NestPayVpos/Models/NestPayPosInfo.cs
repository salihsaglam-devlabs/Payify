namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Models;

public class NestPayPosInfo : IPosInfo
{
    public string MerchantName { get; set; }
    public string Password { get; set; }
    public string StoreKey { get; set; }
    public string NonSecureUrl { get; set; }
    public string ThreeDSecureUrl { get; set; }
    public string ClientId { get; set; }
    public string VisaPfId { get; set; }
    public string VisaSubmerchantPfId { get; set; }
    public int BankCode { get; set; }
}
