using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;

public class ArchiveReconciliationEvaluation : AuditEntity
{
    public Guid FileLineId { get; set; }
    public Guid GroupId { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public int CreatedOperationCount { get; set; }
    public Guid ArchiveRunId { get; set; }
    public DateTime ArchivedAt { get; set; }
    public string ArchivedBy { get; set; }
}
