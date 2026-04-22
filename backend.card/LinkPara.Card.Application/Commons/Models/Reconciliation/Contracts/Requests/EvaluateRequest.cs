using LinkPara.Card.Application.Commons.Models.AppConfiguration;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;

public class EvaluateRequest
{
    public Guid[] IngestionFileIds { get; set; } = Array.Empty<Guid>();

    public CardConfigOptions.EvaluateEndpoint? Options { get; set; }
}
