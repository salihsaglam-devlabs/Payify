namespace LinkPara.PF.Application.Commons.Models.BTrans;

public class ReturnBTransReport
{
    public Guid ReferencePostingTransactionId { get; set; }
    public List<Guid> ReturnPostingTransactionIds { get; set; }
}