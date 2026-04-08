using LinkPara.Card.Domain.Entities.FileIngestion;

namespace LinkPara.Card.Domain.Entities.Archive;

public class ArchiveIngestionFileLine : IngestionFileLine
{
    public DateTime ArchivedAt { get; set; }
}

