namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.VakifVpos.Request;

public class VakifRequestBase
{
    public string MerchantId { get; set; }
    public string Password { get; set; }
    public string TerminalNo { get; set; }
    public bool? IsTopUpPayment { get; set; }
}