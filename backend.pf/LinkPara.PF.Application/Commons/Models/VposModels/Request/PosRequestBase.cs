namespace LinkPara.PF.Application.Commons.Models.VposModels.Request;

public class PosRequestBase
{
    public string LanguageCode { get; set; }
    public string ClientIp { get; set; }
    public bool? IsTopUpPayment { get; set; }
}
