using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveIngestionFile : AuditEntity
{
    public string FileKey { get; set; }
    public string FileName { get; set; }
    public string FullPath { get; set; }
    public string SourceType { get; set; }
    public string FileType { get; set; }
    public string ContentType { get; set; }
    public string Status { get; set; }
    public string Message { get; set; }
    public long ExpectedCount { get; set; }
    public long TotalCount { get; set; }
    public long SuccessCount { get; set; }
    public long ErrorCount { get; set; }
    public long LastProcessedLineNumber { get; set; }
    public long LastProcessedByteOffset { get; set; }
    public bool IsArchived { get; set; }
    public Guid ArchiveRunId { get; set; }
    public DateTime ArchivedAt { get; set; }
    public string ArchivedBy { get; set; }
}

