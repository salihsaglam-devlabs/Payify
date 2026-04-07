using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Engine.Operations;

/// <summary>
/// D7 (Reversal/Void) duzeltmesinin finansal etki adimidir.
/// Islemi iade/geri alma yönunde isaretleyerek reversal effect'in uygulanmasini
/// ve operasyonel iz kaydinin olusmasini saglar.
/// </summary>
internal sealed class ApplyReversalOperationHandler : OperationHandlerBase
{
    public ApplyReversalOperationHandler(
        CardDbContext dbContext,
        IEMoneyService eMoneyTransactionHttpClient,
        ILogger<ApplyReversalOperationHandler> logger,
        IOptions<FileIngestionSettings> options)
        : base(dbContext, eMoneyTransactionHttpClient, logger, options)
    {
    }

    public override string OperationName => "ApplyReversalOperation";

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

        var hasEMoneyTransaction = await HasEMoneyTransactionAsync(plan.OceanTxnGuid, cancellationToken);
        if (!hasEMoneyTransaction)
        {
            return false;
        }

        const string marker = ReconciliationTransactionDescriptionMarkers.ReversalEffectApplied;
        if (!(card.TxnDescription ?? string.Empty).Contains(marker, StringComparison.Ordinal))
        {
            card.TxnDescription = string.IsNullOrWhiteSpace(card.TxnDescription)
                ? marker
                : $"{card.TxnDescription} | {marker}";
            Touch(card, actor);
        }

        // SQL intent (to be executed by EMoneyService endpoint):
        // @customerTransactionId = plan.OceanTxnGuid
        // UPDATE core."transaction"
        // SET transaction_status = 'Returned',
        //     is_cancelled = true,
        //     update_date = NOW(),
        //     last_modified_by = @actor
        // WHERE customer_transaction_id = @customerTransactionId;

        await MarkOperationAsync(plan, scope, "OP:APPLY_REVERSAL_EFFECT", actor, cancellationToken);
        await base.ExecuteAsync(plan, scope, actor, cancellationToken);
        return true;
    }
}
