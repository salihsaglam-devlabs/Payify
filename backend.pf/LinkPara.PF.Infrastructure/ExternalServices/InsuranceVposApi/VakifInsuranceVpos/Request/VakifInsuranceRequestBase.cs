namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Request;

public class VakifInsuranceRequestBase
{
    public string MerchantId { get; set; }
    public string Password { get; set; }
    public string TerminalNo { get; set; }
    public string ClientIp { get; set; }
}