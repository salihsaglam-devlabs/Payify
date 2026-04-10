namespace LinkPara.Card.Application.Commons.Models.Archive.Contracts.Requests;

public class ArchiveRunRequest
{
    public Guid[]? IngestionFileIds { get; set; }

    public DateTime? BeforeDate { get; set; }

    public int? MaxFiles { get; set; }

    public bool? ContinueOnError { get; set; }
}
