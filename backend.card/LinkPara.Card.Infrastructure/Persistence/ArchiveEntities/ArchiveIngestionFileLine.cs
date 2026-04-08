using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Infrastructure.Persistence.ArchiveEntities;

public class ArchiveIngestionFileLine : AuditEntity
{
    public Guid IngestionFileId { get; set; }
    public long LineNumber { get; set; }
    public long ByteOffset { get; set; }
    public int ByteLength { get; set; }
    public string RecordType { get; set; }
    public string RawData { get; set; }
    public string ParsedData { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public int RetryCount { get; set; }
    public string CorrelationKey { get; set; }
    public string CorrelationValue { get; set; }
    public string DuplicateDetectionKey { get; set; }
    public string DuplicateStatus { get; set; }
    public Guid? DuplicateGroupId { get; set; }
    public string ReconciliationStatus { get; set; }
    public Guid ArchiveRunId { get; set; }
    public DateTime ArchivedAt { get; set; }
    public string ArchivedBy { get; set; }
}
