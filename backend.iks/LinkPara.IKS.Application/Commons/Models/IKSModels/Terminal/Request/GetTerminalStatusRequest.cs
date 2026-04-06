namespace LinkPara.IKS.Application.Commons.Models.IKSModels.Terminal.Request;

public class GetTerminalStatusRequest
{
    public string ReferenceCode { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int Offset { get; set; }
    public int Limit { get; set; }
}