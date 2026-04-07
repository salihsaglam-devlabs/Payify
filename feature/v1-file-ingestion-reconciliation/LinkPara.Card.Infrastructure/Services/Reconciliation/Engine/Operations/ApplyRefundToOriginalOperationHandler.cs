using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Engine.Operations;

/// <summary>
/// D4/D5 iade akislarinda, iade islemini orijinal islemle baglayarak
/// refund effect'i uygular. Eslenikli iade kuralinda ana referans (OceanMainTxnGuid)
/// veya cozumlenen orijinal islem uzerinden iliski kurar.
/// </summary>
internal sealed class ApplyRefundToOriginalOperationHandler : OperationHandlerBase
{
    public ApplyRefundToOriginalOperationHandler(
        CardDbContext dbContext,
        IEMoneyService eMoneyTransactionHttpClient,
        ILogger<ApplyRefundToOriginalOperationHandler> logger,
        IOptions<FileIngestionSettings> options)
        : base(dbContext, eMoneyTransactionHttpClient, logger, options)
    {
    }

    public override string OperationName => "ApplyRefundToOriginal";

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
        if (original is null)
        {
            await MarkOperationAsync(plan, scope, "OP:REFUND_ORIGINAL_NOT_FOUND", actor, cancellationToken);
            await base.ExecuteAsync(plan, scope, actor, cancellationToken);
            return true;
        }

        var hasEMoneyTransaction = await HasEMoneyTransactionAsync(original.OceanTxnGuid, cancellationToken);
        if (!hasEMoneyTransaction)
        {
            return false;
        }

        var marker = $"REFUND_BY_ORIGINAL:{original.OceanTxnGuid}";
        if (!(card.TxnDescription ?? string.Empty).Contains(marker, StringComparison.Ordinal))
        {
            card.TxnDescription = string.IsNullOrWhiteSpace(card.TxnDescription)
                ? marker
                : $"{card.TxnDescription} | {marker}";
            Touch(card, actor);
        }

        // SQL intent (to be executed by EMoneyService endpoint):
        // @customerTransactionId = plan.OceanTxnGuid
        // @referenceCustomerTransactionId = original.OceanTxnGuid (resolved from plan.OceanMainTxnGuid/card)
        // @amount = COALESCE(plan.CardHolderBillingAmount, plan.DerivedFields["clearingDestinationAmount"])
        // @currency = COALESCE(plan.CardHolderBillingCurrency, plan.DerivedFields["clearingDestinationCurrency"])
        // UPDATE core."transaction"
        // SET transaction_status = 'Returned',
        //     amount = COALESCE(@amount, amount),
        //     currency_code = COALESCE(NULLIF(@currency, ''), currency_code),
        //     related_transaction_id = (
        //         SELECT id FROM core."transaction"
        //         WHERE customer_transaction_id = @referenceCustomerTransactionId
        //         ORDER BY create_date DESC LIMIT 1
        //     ),
        //     update_date = NOW(),
        //     last_modified_by = @actor
        // WHERE customer_transaction_id = @customerTransactionId;

        Touch(original, actor);
        await MarkOperationAsync(plan, scope, "OP:REFUND_BY_ORIGINAL", actor, cancellationToken);
        await base.ExecuteAsync(plan, scope, actor, cancellationToken);
        return true;
    }
}
