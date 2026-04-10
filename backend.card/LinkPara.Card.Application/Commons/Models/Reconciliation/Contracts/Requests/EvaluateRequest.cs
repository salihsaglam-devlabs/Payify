using LinkPara.Card.Application.Commons.Models.Reconciliation.Configuration;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;

public class EvaluateRequest
{
    public Guid[] IngestionFileIds { get; set; } = Array.Empty<Guid>();

    public EvaluateOptions? Options { get; set; }
}
