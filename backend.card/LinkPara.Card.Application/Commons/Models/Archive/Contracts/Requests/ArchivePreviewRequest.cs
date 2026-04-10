namespace LinkPara.Card.Application.Commons.Models.Archive.Contracts.Requests;

public class ArchivePreviewRequest
{
    public Guid[]? IngestionFileIds { get; set; }

    public DateTime? BeforeDate { get; set; }

    public int? Limit { get; set; }
}
