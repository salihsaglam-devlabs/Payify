namespace LinkPara.Card.Application.Commons.Models.Archive;

public class ArchiveRunRequest
{
    public Guid[]? FileIds { get; set; }

    public DateTime? BeforeDate { get; set; }

    public int? MaxFiles { get; set; }

    public bool? ContinueOnError { get; set; }
}
