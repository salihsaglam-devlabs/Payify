namespace LinkPara.PF.Infrastructure.ExternalServices.InsuranceVposApi.VakifInsuranceVpos.Response;

public class VakifInsuranceResponseBase
{
    public string MerchantId { get; set; }
    public string TransactionId { get; set; }
    public string ResultCode { get; set; }
    public string ResultDetail { get; set; }
    public string AuthCode { get; set; }
    public string Rrn { get; set; }
    public string HostDate { get; set; }
}