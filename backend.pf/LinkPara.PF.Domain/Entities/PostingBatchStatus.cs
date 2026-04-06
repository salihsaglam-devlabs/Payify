using LinkPara.PF.Domain.Enums;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.PF.Domain.Entities;
public class PostingBatchStatus : AuditEntity
{
    public PostingBatchLevel PostingBatchLevel { get; set; }
    public string BatchSummary { get; set; }
    public bool IsCriticalError { get; set; }
    public DateTime PostingDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime FinishTime { get; set; }
    public BatchStatus BatchStatus { get; set; }
    public int BatchOrder { get; set; }
}