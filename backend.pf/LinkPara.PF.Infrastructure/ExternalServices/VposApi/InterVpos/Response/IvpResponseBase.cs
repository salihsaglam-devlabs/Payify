namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.InterVpos.Response;

public class IvpResponseBase
{
    public string TransId { get; set; }
    public string ProcReturnCode { get; set; }
    public string HostRefNum { get; set; }
    public string AuthCode { get; set; }
    public string TxnResult { get; set; }
    public string ErrorMessage { get; set; }
    public DateTime TrxDate { get; set; }
}
