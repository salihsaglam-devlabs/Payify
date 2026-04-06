namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.NestPayVpos.Request;

public class NestPaymentBase : NestPayBaseRequest
{
    public string OkUrl { get; set; }
    public string FailUrl { get; set; }
    public string LanguageCode { get; set; }
    public string Rnd { get; set; }
    public string StoreType { get; set; }
    public string ExpireMonth { get; set; }
    public string ExpireYear { get; set; }
    public string HashAlgorithm { get; set; }
    public string StoreKey { get; set; }
    public string Hash { get; set; }
}
