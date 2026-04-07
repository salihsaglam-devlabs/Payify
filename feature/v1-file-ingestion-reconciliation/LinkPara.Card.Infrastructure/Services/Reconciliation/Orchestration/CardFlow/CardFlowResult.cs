using System.Collections.Generic;
using LinkPara.Card.Domain.Enums;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration.CardFlow;

internal sealed class CardFlowResult
{
    public Guid CardTransactionRecordId { get; init; }
    public Guid? ClearingRecordId { get; init; }
    public IReadOnlyList<FlowOperation> Operations { get; init; } = [];
}

internal sealed class FlowOperation
{
    public ReconciliationOperationCode Code { get; init; }
    public ReconciliationOperationMode Mode { get; init; }
    public int Index { get; init; }
    public string ReasonCode { get; init; } = string.Empty;
    public string ReasonText { get; init; } = string.Empty;
    public IReadOnlyDictionary<string, string> Metadata { get; init; } = new Dictionary<string, string>();
}
