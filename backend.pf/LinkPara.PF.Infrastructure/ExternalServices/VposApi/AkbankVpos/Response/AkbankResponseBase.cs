namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.AkbankVpos.Response;

public class AkbankResponseBase
{
    public string ResponseCode { get; set; }
    public string ResponseMessage { get; set; }
    public string HostResponseCode { get; set; }
    public string HostMessage { get; set; }
    public string AuthCode { get; set; }
    public string RrnNumber { get; set; }
    public DateTime TxnDateTime { get; set; }
    public string TxnCode { get; set; }
    public string OrderId { get; set; }
    public string TxnStatus { get; set; }
    public string Amount { get; set; }
    public int InstallCount { get; set; }
}
