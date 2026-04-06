namespace LinkPara.PF.Application.Commons.Models.PhysicalPos;

public class PhysicalPosReconciliation
{
    public Guid PhysicalPosEodId { get; set; }
    public List<Guid> ReconciliationTransactionIds { get; set; }
}