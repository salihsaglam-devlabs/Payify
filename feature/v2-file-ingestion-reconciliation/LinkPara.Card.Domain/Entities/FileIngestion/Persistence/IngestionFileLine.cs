using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.FileIngestion;

public class IngestionFileLine : AuditEntity
{
    public Guid IngestionFileId { get; set; }
    public IngestionFile IngestionFile { get; set; }
    public long LineNumber { get; set; }
    public long ByteOffset { get; set; }
    public int ByteLength { get; set; }
    public string RecordType { get; set; }
    public string RawData { get; set; }
    public string ParsedData { get; set; }
    public FileRowStatus Status { get; set; }
    public string Message { get; set; }
    public int RetryCount { get; set; }
    public string CorrelationKey { get; set; }
    public string CorrelationValue { get; set; }
    public string DuplicateStatus { get; set; }
    public Guid? DuplicateGroupId { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
}
