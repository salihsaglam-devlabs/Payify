using LinkPara.SharedModels.Persistence;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveLog : AuditEntity
{
    public Guid IngestionFileId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Message { get; set; }
    public string? FailureReasonsJson { get; set; }
    public string? FilterJson { get; set; }
}

