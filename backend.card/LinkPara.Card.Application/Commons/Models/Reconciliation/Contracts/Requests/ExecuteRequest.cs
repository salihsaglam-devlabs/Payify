using LinkPara.Card.Application.Commons.Models.AppConfiguration;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;

public class ExecuteRequest
{
    public Guid[] GroupIds { get; set; } = Array.Empty<Guid>();

    public Guid[] EvaluationIds { get; set; } = Array.Empty<Guid>();

    public Guid[] OperationIds { get; set; } = Array.Empty<Guid>();

    public CardConfigOptions.OperationsExecuteEndpoint? Options { get; set; }
}
