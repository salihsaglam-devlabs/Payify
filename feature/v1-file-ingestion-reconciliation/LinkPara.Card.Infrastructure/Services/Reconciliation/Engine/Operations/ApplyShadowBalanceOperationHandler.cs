using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Engine.Operations;

/// <summary>
/// D8 (Amount/BillingAmount uyumsuzlugu) icin shadow balance duzeltme adimidir.
/// Dokuman kuralina gore ozellikle <c>transaction amount &lt; billingAmount</c> durumunda
/// farkin borc/alacak etkisine tasinmasi ve ilgili statu izlerinin yazilmasi hedeflenir.
/// </summary>
internal sealed class ApplyShadowBalanceOperationHandler : OperationHandlerBase
{
    public ApplyShadowBalanceOperationHandler(
        CardDbContext dbContext,
        IEMoneyService eMoneyTransactionHttpClient,
        ILogger<ApplyShadowBalanceOperationHandler> logger,
        IOptions<FileIngestionSettings> options)
        : base(dbContext, eMoneyTransactionHttpClient, logger, options)
    {
    }

    public override string OperationName => "ApplyShadowBalanceOperation";

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

        var isExpireFlow = string.Equals(
            (card.TxnStat ?? string.Empty).Trim(),
            ReconciliationFieldValues.TxnStatExpire,
            StringComparison.OrdinalIgnoreCase);

        if (isExpireFlow)
        {
            card.TxnStat = CardLookupCodes.TxnStat.Expire;
            Touch(card, actor);

            await MarkOperationAsync(plan, scope, "OP:SET_EXPIRE_STATUS_FOR_SHADOW_BALANCE", actor, cancellationToken);

            var hasEMoneyTransaction = await HasEMoneyTransactionAsync(plan.OceanTxnGuid, cancellationToken);
            if (!hasEMoneyTransaction)
            {
                // SQL intent (to be executed by EMoneyService endpoint):
                // @customerTransactionId = plan.OceanTxnGuid
                // @referenceCustomerTransactionId = COALESCE(plan.OceanMainTxnGuid, plan.OceanTxnGuid)
                // @amount = COALESCE(plan.CardHolderBillingAmount, plan.DerivedFields["clearingDestinationAmount"])
                // @currency = COALESCE(plan.CardHolderBillingCurrency, plan.DerivedFields["clearingDestinationCurrency"])
                // INSERT INTO core."transaction"
                // (id, transaction_status, currency_code, amount, customer_transaction_id, transaction_date, create_date, created_by, record_status, wallet_id, transaction_type, payment_method, pre_balance, current_balance, transaction_direction)
                // SELECT
                //   gen_random_uuid(),
                //   'Failed',
                //   COALESCE(NULLIF(@currency, ''), ref.currency_code),
                //   COALESCE(@amount, ref.amount),
                //   @customerTransactionId,
                //   NOW(),
                //   NOW(),
                //   @actor,
                //   'Active',
                //   ref.wallet_id,
                //   ref.transaction_type,
                //   ref.payment_method,
                //   ref.current_balance,
                //   ref.current_balance,
                //   ref.transaction_direction
                // FROM core."transaction" ref
                // WHERE ref.customer_transaction_id = @referenceCustomerTransactionId
                //   AND NOT EXISTS (
                //     SELECT 1 FROM core."transaction" t
                //     WHERE t.customer_transaction_id = @customerTransactionId
                //   )
                // ORDER BY ref.create_date DESC
                // LIMIT 1;
                await MarkOperationAsync(plan, scope, "OP:CREATE_TXN_REQUESTED_FOR_SHADOW_BALANCE", actor, cancellationToken);
            }
            else
            {
                // SQL intent (to be executed by EMoneyService endpoint):
                // @customerTransactionId = plan.OceanTxnGuid
                // UPDATE core."transaction"
                // SET transaction_status = 'Failed',
                //     update_date = NOW(),
                //     last_modified_by = @actor
                // WHERE customer_transaction_id = @customerTransactionId;
            }

            await base.ExecuteAsync(plan, scope, actor, cancellationToken);
            return true;
        }

        const string marker = ReconciliationTransactionDescriptionMarkers.ShadowBalanceRequired;
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
        // SET transaction_status = 'Completed',
        //     is_settlement_received = true,
        //     update_date = NOW(),
        //     last_modified_by = @actor
        // WHERE customer_transaction_id = @customerTransactionId;

        await MarkOperationAsync(plan, scope, "OP:APPLY_SHADOW_BALANCE", actor, cancellationToken);
        await base.ExecuteAsync(plan, scope, actor, cancellationToken);
        return true;
    }
}
