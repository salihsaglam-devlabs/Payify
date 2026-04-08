namespace LinkPara.Card.Application.Commons.Models.Archive;

public class ArchivePreviewRequest
{
    public Guid[]? FileIds { get; set; }

    public DateTime? BeforeDate { get; set; }

    public int? Limit { get; set; }
}
