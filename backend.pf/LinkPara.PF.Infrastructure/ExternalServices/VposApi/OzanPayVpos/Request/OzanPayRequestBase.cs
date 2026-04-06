namespace LinkPara.PF.Infrastructure.ExternalServices.VposApi.OzanPayVpos.Request;

public class OzanPayRequestBase
{
    public string ApiKey { get; set; }
    public string ReferenceNo { get; set; }
    public string TransactionId { get; set; }
}
