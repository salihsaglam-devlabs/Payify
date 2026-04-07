using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Engine.Operations;

/// <summary>
/// D7 (Reversal/Void) akisinda orijinal islemi iptal edilmis olarak isaretler.
/// Orijinal kaydin TxnStat bilgisini void/cancel mantigina ceker ve
/// sonrasinda reversal effect adiminin tutarli calismasi icin zemin hazirlar.
/// </summary>
internal sealed class MarkOriginalCancelledOperationHandler : OperationHandlerBase
{
    public MarkOriginalCancelledOperationHandler(
        CardDbContext dbContext,
        IEMoneyService eMoneyTransactionHttpClient,
        ILogger<MarkOriginalCancelledOperationHandler> logger,
        IOptions<FileIngestionSettings> options)
        : base(dbContext, eMoneyTransactionHttpClient, logger, options)
    {
    }

    public override string OperationName => "MarkOriginalCancelled";

    public override Task<bool> ExecuteAsync(
        ReconciliationOperationPlan plan,
        ReconciliationOperationScope scope,
        string actor,
        CancellationToken cancellationToken = default)
    {
        return ExecuteInternalAsync(plan, scope, actor, cancellationToken);
    }

    private async Task<bool> ExecuteInternalAsync(
        ReconciliationOperationPlan plan,
        ReconciliationOperationScope scope,
        string actor,
        CancellationToken cancellationToken)
    {
        var card = await GetCardAsync(scope, cancellationToken);
        if (card is null)
        {
            return false;
        }

        var original = await GetOriginalCardAsync(card, cancellationToken);
        if (original is not null)
        {
            var hasEMoneyTransaction = await HasEMoneyTransactionAsync(original.OceanTxnGuid, cancellationToken);
            if (!hasEMoneyTransaction)
            {
                return false;
            }

            original.TxnStat = CardLookupCodes.TxnStat.Void;
            const string marker = ReconciliationTransactionDescriptionMarkers.OriginalCancelledByD7;
            if (!(original.TxnDescription ?? string.Empty).Contains(marker, StringComparison.Ordinal))
            {
                original.TxnDescription = string.IsNullOrWhiteSpace(original.TxnDescription)
                    ? marker
                    : $"{original.TxnDescription} | {marker}";
            }

            // SQL intent (to be executed by EMoneyService endpoint):
            // @customerTransactionId = original.OceanTxnGuid
            // UPDATE core."transaction"
            // SET transaction_status = 'Rejected',
            //     is_cancelled = true,
            //     update_date = NOW(),
            //     last_modified_by = @actor
            // WHERE customer_transaction_id = @customerTransactionId;

            Touch(original, actor);
        }
        await MarkOperationAsync(plan, scope, "OP:MARK_ORIGINAL_CANCELLED", actor, cancellationToken);
        await base.ExecuteAsync(plan, scope, actor, cancellationToken);
        return true;
    }
}
