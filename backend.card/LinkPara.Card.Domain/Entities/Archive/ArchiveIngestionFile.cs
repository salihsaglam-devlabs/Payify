using LinkPara.Card.Domain.Entities.FileIngestion;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveIngestionFile : IngestionFile
{
    public DateTime ArchivedAt { get; set; }
}

