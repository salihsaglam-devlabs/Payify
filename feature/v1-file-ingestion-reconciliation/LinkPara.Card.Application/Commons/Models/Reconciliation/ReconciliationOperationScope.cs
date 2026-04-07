namespace LinkPara.Card.Application.Commons.Models.Reconciliation;

public sealed class ReconciliationOperationScope
{
    public Guid CardTransactionRecordId { get; init; }
    public Guid? ClearingRecordId { get; init; }
    public Guid RunId { get; init; }
}
