using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveBatchItem : AuditEntity
{
    public Guid BatchId { get; set; }
    public Guid IngestionFileId { get; set; }
    public Guid? ArchiveRunId { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public string FailureReasonsJson { get; set; }
    public DateTime ProcessedAt { get; set; }
}

