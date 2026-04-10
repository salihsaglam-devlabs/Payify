using LinkPara.Card.Application.Commons.Models.Reconciliation.Configuration;

namespace LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Requests;

public class ExecuteRequest
{
    public Guid[] GroupIds { get; set; } = Array.Empty<Guid>();

    public Guid[] EvaluationIds { get; set; } = Array.Empty<Guid>();

    public Guid[] OperationIds { get; set; } = Array.Empty<Guid>();

    public ExecuteOptions? Options { get; set; }
}
