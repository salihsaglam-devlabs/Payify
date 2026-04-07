using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Engine.Operations;

/// <summary>
/// D6 (Esleniksiz Iade / UnlinkedRefundManualReview) senaryosunda manuel inceleme
/// <c>Approve</c> edildiginde iade etkisini uygulayan operation'dir.
/// Islem, orijinal referansa bagli refund effect execution adimini temsil eder.
/// </summary>
internal sealed class ApplyManualRefundIfApprovedOperationHandler : OperationHandlerBase
{
    public ApplyManualRefundIfApprovedOperationHandler(
        CardDbContext dbContext,
        IEMoneyService eMoneyTransactionHttpClient,
        ILogger<ApplyManualRefundIfApprovedOperationHandler> logger,
        IOptions<FileIngestionSettings> options)
        : base(dbContext, eMoneyTransactionHttpClient, logger, options)
    {
    }

    public override string OperationName => "ApplyManualRefundIfApproved";

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
        var referenceGuid = string.IsNullOrWhiteSpace(plan.OceanMainTxnGuid) ? plan.OceanTxnGuid : plan.OceanMainTxnGuid;
        var hasEMoneyTransaction = await HasEMoneyTransactionAsync(referenceGuid, cancellationToken);
        if (!hasEMoneyTransaction)
        {
            return false;
        }

        // SQL intent (to be executed by EMoneyService endpoint):
        // @customerTransactionId = plan.OceanTxnGuid
        // @referenceCustomerTransactionId = COALESCE(plan.OceanMainTxnGuid, plan.OceanTxnGuid)
        // UPDATE core."transaction"
        // SET transaction_status = 'Returned',
        //     related_transaction_id = (
        //         SELECT id FROM core."transaction"
        //         WHERE customer_transaction_id = @referenceCustomerTransactionId
        //         ORDER BY create_date DESC LIMIT 1
        //     ),
        //     update_date = NOW(),
        //     last_modified_by = @actor
        // WHERE customer_transaction_id = @customerTransactionId;

        await MarkOperationAsync(plan, scope, "OP:APPLY_MANUAL_REFUND_IF_APPROVED", actor, cancellationToken);
        await base.ExecuteAsync(plan, scope, actor, cancellationToken);
        return true;
    }
}
