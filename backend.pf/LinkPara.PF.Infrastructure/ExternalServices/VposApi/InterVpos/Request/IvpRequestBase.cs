namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Request;

public class IvpRequestBase
{
    public string ShopCode { get; set; }
    public string UserCode { get; set; }
    public string UserPass { get; set; }
    public string Lang { get; set; } = "tr";
    public string TxnType { get; set; }
    public string SecureType { get; set; }
}
