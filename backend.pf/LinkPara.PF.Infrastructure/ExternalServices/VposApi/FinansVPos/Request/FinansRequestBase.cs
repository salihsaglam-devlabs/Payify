namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.FinansVPos.Request;
public class FinansRequestBase
{
    public string UserCode { get; set; }
    public string UserPass { get; set; }
    public string MerchantId { get; set; }
    public string MerchantPass { get; set; }
    public string LanguageCode { get; set; }
    public string MbrId { get; set; }
}
