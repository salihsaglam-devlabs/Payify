using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Engine.Operations;

/// <summary>
/// D4 senaryosunda (Card dosyasi basarili, Prepaid tarafta islem yok)
/// eksik islemin olusturulmasi adimini temsil eder.
/// Islem referans, tutar, para birimi ve bakiye baglamini kullanarak yeni transaction
/// acma niyetini (integration placeholder) kayda alir.
/// </summary>
internal sealed class CreateTransactionOperationHandler : OperationHandlerBase
{
    public CreateTransactionOperationHandler(
        CardDbContext dbContext,
        IEMoneyService eMoneyTransactionHttpClient,
        ILogger<CreateTransactionOperationHandler> logger,
        IOptions<FileIngestionSettings> options)
        : base(dbContext, eMoneyTransactionHttpClient, logger, options)
    {
    }

    public override string OperationName => "CreateTransaction";

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

        var createSucceeded = await EMoneyTransactionHttpClient.CreateTransactionForCardReconciliationAsync(
            card.OceanTxnGuid,
            card.OceanMainTxnGuid,
            card.BillingAmount ?? card.CardHolderBillingAmount,
            card.BillingCurrency ?? card.CardHolderBillingCurrency,
            ResolveOperationIdempotencyKey(plan),
            cancellationToken);
        if (!createSucceeded)
        {
            return false;
        }

        // SQL intent (to be executed by EMoneyService endpoint):
        // @customerTransactionId = plan.OceanTxnGuid
        // @referenceCustomerTransactionId = COALESCE(plan.OceanMainTxnGuid, plan.OceanTxnGuid)
        // @amount = COALESCE(plan.CardHolderBillingAmount, plan.DerivedFields["clearingDestinationAmount"])
        // @currency = COALESCE(plan.CardHolderBillingCurrency, plan.DerivedFields["clearingDestinationCurrency"])
        // @externalReferenceId = COALESCE(plan.Rrn, plan.Arn, plan.ProvisionCode)
        // @walletBalanceSource = reference transaction (or wallet lookup) for wallet_id, pre_balance, current_balance
        // INSERT INTO core."transaction"
        // (id, transaction_status, currency_code, amount, customer_transaction_id, external_reference_id, transaction_date, create_date, created_by, record_status, wallet_id, transaction_type, payment_method, pre_balance, current_balance, transaction_direction)
        // SELECT
        //   gen_random_uuid(),
        //   'Completed',
        //   COALESCE(NULLIF(@currency, ''), ref.currency_code),
        //   COALESCE(@amount, ref.amount),
        //   @customerTransactionId,
        //   COALESCE(NULLIF(@externalReferenceId, ''), ref.external_reference_id),
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

        await MarkOperationAsync(plan, scope, "OP:CREATE_TXN_REQUESTED", actor, cancellationToken);
        await base.ExecuteAsync(plan, scope, actor, cancellationToken);
        return true;
    }
}
