using LinkPara.Card.Domain.Entities.FileIngestion;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveIngestionFile : IngestionFile
{
    public DateTime ArchivedAt { get; set; }
    public string ArchivedBy { get; set; } = string.Empty;
    public Guid ArchiveRunId { get; set; }
    public DateTime? ArchiveRecordWrittenAt { get; set; }
    public Guid? ArchiveRecordRunId { get; set; }
    public DateTime? ArchiveChildrenTransitionedAt { get; set; }
    public DateTime? ArchiveCleanupEligibleAt { get; set; }
    public DateTime? ArchiveCleanupCompletedAt { get; set; }
}

