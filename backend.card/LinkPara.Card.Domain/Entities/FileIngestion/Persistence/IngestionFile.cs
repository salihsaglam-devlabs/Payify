using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.FileIngestion;

public class IngestionFile : AuditEntity
{
    public string FileKey { get; set; }
    public string FileName { get; set; }
    public string FullPath { get; set; }
    public FileSourceType SourceType { get; set; }
    public FileType FileType { get; set; }
    public FileContentType ContentType { get; set; }
    public FileStatus Status { get; set; }
    public string Message { get; set; }
    public long ExpectedCount { get; set; }
    public long TotalCount { get; set; }
    public long SuccessCount { get; set; }
    public long ErrorCount { get; set; }
    public long LastProcessedLineNumber { get; set; }
    public long LastProcessedByteOffset { get; set; }
    public bool IsArchived { get; set; }
}
