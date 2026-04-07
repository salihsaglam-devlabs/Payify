using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Models.Reconciliation;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration.CardFlow;

internal sealed class PrepaidLookupInfo
{
    public PrepaidTransactionStatus Status { get; init; }
    public bool IsCancelled { get; init; }
    public decimal? Amount { get; init; }

    public static PrepaidLookupInfo Unknown()
    {
        return new PrepaidLookupInfo
        {
            Status = PrepaidTransactionStatus.Unknown,
            IsCancelled = false,
            Amount = null
        };
    }
}

internal enum PrepaidTransactionStatus
{
    Unknown = 0,
    Missing = 1,
    Failed = 2,
    Successful = 3
}

internal static class PrepaidLookupResolver
{
    public static bool IsCancelledState(string transactionState)
    {
        var normalized = CardFlowText.Normalize(transactionState);
        return normalized.Contains(ReconciliationFlowValues.CancelToken, StringComparison.Ordinal)
               || normalized.Contains(ReconciliationFlowValues.ReverseToken, StringComparison.Ordinal)
               || normalized.Contains(ReconciliationFlowValues.VoidToken, StringComparison.Ordinal);
    }

    public static PrepaidTransactionStatus ResolvePrepaidStatus(EMoneyTransactionLookupResult lookup)
    {
        if (lookup is null || lookup.Status == EMoneyTransactionLookupStatus.Unavailable)
        {
            return PrepaidTransactionStatus.Unknown;
        }

        if (lookup.Status == EMoneyTransactionLookupStatus.NotFound)
        {
            return PrepaidTransactionStatus.Missing;
        }

        var state = CardFlowText.Normalize(lookup.TransactionState);
        if (state is ReconciliationFieldValues.EMoneyStateFailed
            or ReconciliationFieldValues.EMoneyStateReject
            or ReconciliationFieldValues.EMoneyStateRejected
            or ReconciliationFieldValues.EMoneyStateDeclined)
        {
            return PrepaidTransactionStatus.Failed;
        }

        return PrepaidTransactionStatus.Successful;
    }
}
