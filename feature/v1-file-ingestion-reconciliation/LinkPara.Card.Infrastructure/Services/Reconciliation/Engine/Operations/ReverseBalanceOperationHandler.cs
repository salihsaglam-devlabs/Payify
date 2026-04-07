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
/// D1/D2/D3 gibi "bakiye etkisini geri alma" gereken duzeltme kararlarinda kullanilir.
/// Isleme daha once uygulanmis finansal etkiyi ters yone cevirerek
/// Card dosyasi ile Prepaid kaydi arasindaki bakiye tutarsizligini kapatmayi amaclar.
/// </summary>
internal sealed class ReverseBalanceOperationHandler : OperationHandlerBase
{
    public ReverseBalanceOperationHandler(
        CardDbContext dbContext,
        IEMoneyService eMoneyTransactionHttpClient,
        ILogger<ReverseBalanceOperationHandler> logger,
        IOptions<FileIngestionSettings> options)
        : base(dbContext, eMoneyTransactionHttpClient, logger, options)
    {
    }

    public override string OperationName => "ReverseBalanceOperation";

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

        var marker = string.Equals((card.TxnEffect ?? string.Empty).Trim(), CardLookupCodes.TxnEffect.Refund, StringComparison.OrdinalIgnoreCase)
            ? ReconciliationTransactionDescriptionMarkers.OriginalEffectReversed
            : ReconciliationTransactionDescriptionMarkers.BalanceEffectReversed;
        var hasEMoneyTransaction = await HasEMoneyTransactionAsync(plan.OceanTxnGuid, cancellationToken);
        if (!hasEMoneyTransaction)
        {
            return false;
        }

        if (!(card.TxnDescription ?? string.Empty).Contains(marker, StringComparison.Ordinal))
        {
            card.TxnDescription = string.IsNullOrWhiteSpace(card.TxnDescription)
                ? marker
                : $"{card.TxnDescription} | {marker}";
            Touch(card, actor);
        }

        // SQL intent (to be executed by EMoneyService endpoint):
        // @customerTransactionId = plan.OceanTxnGuid
        // @targetStatus = (marker == "ORIGINAL_EFFECT_REVERSED" ? 'Returned' : 'Completed')
        // UPDATE core."transaction"
        // SET transaction_status = @targetStatus, update_date = NOW(), last_modified_by = @actor
        // WHERE customer_transaction_id = @customerTransactionId;

        await MarkOperationAsync(plan, scope, "OP:REVERSE_BALANCE_EFFECT", actor, cancellationToken);
        await base.ExecuteAsync(plan, scope, actor, cancellationToken);
        return true;
    }
}
