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
/// D1/D3 duzeltme senaryolarinda CardTransaction kaydinin cevap kodunu (ResponseCode)
/// ve basari bayragini (IsSuccessfulTxn) dokuman akisina uygun sekilde gunceller.
/// Amac, Card dosyasi sonucu ile Prepaid tarafi arasindaki statu uyumsuzlugunu gidermektir.
/// </summary>
internal sealed class AdjustResponseCodeOperationHandler : OperationHandlerBase
{
    public AdjustResponseCodeOperationHandler(
        CardDbContext dbContext,
        IEMoneyService eMoneyTransactionHttpClient,
        ILogger<AdjustResponseCodeOperationHandler> logger,
        IOptions<FileIngestionSettings> options)
        : base(dbContext, eMoneyTransactionHttpClient, logger, options)
    {
    }

    public override string OperationName => "AdjustResponseCode";

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

        var transition = plan.DerivedFields.TryGetValue(ReconciliationDerivedFieldKeys.ResponseCodeTransition, out var transitionValue)
            ? transitionValue
            : ReconciliationDerivedFieldValues.NoChange;
        var hasEMoneyTransaction = await HasEMoneyTransactionAsync(plan.OceanTxnGuid, cancellationToken);
        if (!hasEMoneyTransaction)
        {
            return false;
        }

        if (transition == ReconciliationDerivedFieldValues.SuccessToFailed)
        {
            card.ResponseCode = ReconciliationFieldValues.ResponseCodeDeclined;
            card.IsSuccessfulTxn = CardLookupCodes.Flag.No;
            // SQL intent (to be executed by EMoneyService endpoint):
            // @customerTransactionId = plan.OceanTxnGuid
            // @settlementReceived = (plan.IsTxnSettle == "Y")
            // UPDATE core."transaction"
            // SET transaction_status = 'Failed',
            //     is_settlement_received = @settlementReceived,
            //     update_date = NOW(),
            //     last_modified_by = @actor
            // WHERE customer_transaction_id = @customerTransactionId;
        }
        else if (transition == ReconciliationDerivedFieldValues.FailedToSuccess)
        {
            card.ResponseCode = ReconciliationFieldValues.ResponseCodeApproved;
            card.IsSuccessfulTxn = CardLookupCodes.Flag.Yes;
            // SQL intent (to be executed by EMoneyService endpoint):
            // @customerTransactionId = plan.OceanTxnGuid
            // @settlementReceived = (plan.IsTxnSettle == "Y")
            // UPDATE core."transaction"
            // SET transaction_status = 'Completed',
            //     is_settlement_received = @settlementReceived,
            //     update_date = NOW(),
            //     last_modified_by = @actor
            // WHERE customer_transaction_id = @customerTransactionId;
        }
        else
        {
            // SQL intent: no mutation on core."transaction".
        }

        Touch(card, actor);
        await MarkOperationAsync(plan, scope, "OP:ADJUST_RESPONSE_CODE", actor, cancellationToken);
        await base.ExecuteAsync(plan, scope, actor, cancellationToken);
        return true;
    }
}
