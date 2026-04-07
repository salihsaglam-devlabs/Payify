namespace LinkPara.Card.Application.Commons.Models.Logging;

public class BulkServiceOperationLog
{
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public string ServiceName { get; set; }
    public string EndpointName { get; set; }
    public string Actor { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime EndedAt { get; set; }
    public long DurationMs { get; set; }
    public bool IsSuccess { get; set; }
    public string Summary { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public List<string> Logs { get; set; } = [];
}
