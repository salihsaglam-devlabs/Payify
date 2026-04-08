using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;

public class ArchiveBatch : AuditEntity
{
    public DateTime RequestedAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string RequestedBy { get; set; }
    public string FilterJson { get; set; }
    public string Status { get; set; }
    public int ProcessedCount { get; set; }
    public int ArchivedCount { get; set; }
    public int SkippedCount { get; set; }
    public int FailedCount { get; set; }
}
