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
/// D2 akisinda islemi EXPIRE statüsüne cekmek icin kullanilan operation'dir.
/// Card dosyasinda expire gelen kaydin, Prepaid tarafta beklenen statu ile
/// hizalanmasini saglar ve sonraki bakiye tersleme adimi icin temel olusturur.
/// </summary>
internal sealed class SetExpireStatusOperationHandler : OperationHandlerBase
{
    public SetExpireStatusOperationHandler(
        CardDbContext dbContext,
        IEMoneyService eMoneyTransactionHttpClient,
        ILogger<SetExpireStatusOperationHandler> logger,
        IOptions<FileIngestionSettings> options)
        : base(dbContext, eMoneyTransactionHttpClient, logger, options)
    {
    }

    public override string OperationName => "SetExpireStatus";

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

        var expireSucceeded = await EMoneyTransactionHttpClient.SetExpireStatusByCustomerTransactionIdAsync(
            plan.OceanTxnGuid,
            ResolveOperationIdempotencyKey(plan),
            cancellationToken);
        if (!expireSucceeded)
        {
            return false;
        }

        // SQL intent (to be executed by EMoneyService endpoint):
        // @customerTransactionId = plan.OceanTxnGuid
        // UPDATE core."transaction"
        // SET transaction_status = 'Failed',
        //     update_date = NOW(),
        //     last_modified_by = @actor
        // WHERE customer_transaction_id = @customerTransactionId;

        card.TxnStat = CardLookupCodes.TxnStat.Expire;
        Touch(card, actor);
        await MarkOperationAsync(plan, scope, "OP:SET_EXPIRE_STATUS", actor, cancellationToken);
        await base.ExecuteAsync(plan, scope, actor, cancellationToken);
        return true;
    }
}
