using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Engine.Operations;

internal abstract class OperationHandlerBase : IReconciliationOperationHandler
{
    protected readonly CardDbContext DbContext;
    protected readonly IEMoneyService EMoneyTransactionHttpClient;
    private readonly ILogger _logger;
    private readonly int _matchingLookbackDays;

    protected OperationHandlerBase(
        CardDbContext dbContext,
        IEMoneyService eMoneyTransactionHttpClient,
        ILogger logger,
        IOptions<FileIngestionSettings> options)
    {
        DbContext = dbContext;
        EMoneyTransactionHttpClient = eMoneyTransactionHttpClient;
        _logger = logger;
        var configuredLookbackDays = options?.Value?.ReconciliationProcessing?.OriginalTxnMatchingLookbackDays ?? 0;
        _matchingLookbackDays = configuredLookbackDays > 0 ? configuredLookbackDays : 10;
    }

    public abstract string OperationName { get; }

    public virtual Task<bool> ExecuteAsync(
        ReconciliationOperationPlan plan,
        ReconciliationOperationScope scope,
        string actor,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Reconciliation operation executed. Operation={Operation}, CardId={CardId}, RunId={RunId}",
            OperationName,
            scope.CardTransactionRecordId,
            scope.RunId);
        return Task.FromResult(true);
    }

    protected async Task<CardTransactionRecord> GetCardAsync(ReconciliationOperationScope scope, CancellationToken cancellationToken)
    {
        return await DbContext.CardTransactionRecords
            .AsTracking()
            .FirstOrDefaultAsync(x => x.Id == scope.CardTransactionRecordId, cancellationToken);
    }

    protected async Task<CardTransactionRecord> GetOriginalCardAsync(CardTransactionRecord card, CancellationToken cancellationToken)
    {
        var matchingLookbackDate = DateTime.Now.AddDays(-_matchingLookbackDays);
        var referenceGuid = string.IsNullOrWhiteSpace(card.OceanMainTxnGuid)
            ? card.OceanTxnGuid
            : card.OceanMainTxnGuid;

        if (string.IsNullOrWhiteSpace(referenceGuid))
        {
            return null;
        }

        var normalized = referenceGuid.Trim().ToUpperInvariant();

        return await DbContext.CardTransactionRecords
            .AsTracking()
            .Where(x => x.Id != card.Id)
            .Where(x => x.CreateDate >= matchingLookbackDate)
            .Where(x => (x.OceanTxnGuid ?? string.Empty).Trim().ToUpper() == normalized)
            .OrderByDescending(x => x.CreateDate)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected async Task MarkOperationAsync(
        ReconciliationOperationPlan plan,
        ReconciliationOperationScope scope,
        string operationMarker,
        string actor,
        CancellationToken cancellationToken)
    {
        ReconciliationOperation operation = null;
        if (plan.DerivedFields.TryGetValue(ReconciliationDerivedFieldKeys.CurrentOperationId, out var currentOperationIdRaw)
            && Guid.TryParse(currentOperationIdRaw, out var currentOperationId))
        {
            operation = await DbContext.ReconciliationOperations
                .AsTracking()
                .FirstOrDefaultAsync(x =>
                        x.Id == currentOperationId &&
                        x.CardTransactionRecordId == scope.CardTransactionRecordId &&
                        x.RunId == scope.RunId,
                    cancellationToken);
        }

        operation ??= await DbContext.ReconciliationOperations
            .AsTracking()
            .Where(x =>
                x.CardTransactionRecordId == scope.CardTransactionRecordId &&
                x.RunId == scope.RunId)
            .OrderByDescending(x => x.CreateDate)
            .ThenByDescending(x => x.OperationIndex)
            .FirstOrDefaultAsync(cancellationToken);

        if (operation is null)
        {
            return;
        }

        var marker = $"[{operationMarker}]";
        var payload = EnsurePayloadJson(operation.Payload);
        var markers = payload["executionMarkers"] as JsonArray;
        if (markers is null)
        {
            markers = new JsonArray();
            payload["executionMarkers"] = markers;
        }

        if (!markers.Any(node => string.Equals(node?.GetValue<string>(), marker, StringComparison.Ordinal)))
        {
            markers.Add(marker);
        }

        operation.Payload = payload.ToJsonString(new JsonSerializerOptions { WriteIndented = false });
        operation.UpdateDate = DateTime.Now;
        operation.LastModifiedBy = actor;
    }

    private static JsonObject EnsurePayloadJson(string payloadRaw)
    {
        if (string.IsNullOrWhiteSpace(payloadRaw))
        {
            return new JsonObject();
        }

        try
        {
            var parsed = JsonNode.Parse(payloadRaw) as JsonObject;
            if (parsed is not null)
            {
                return parsed;
            }
        }
        catch (JsonException)
        {
            // Ignore and wrap raw payload.
        }

        return new JsonObject
        {
            ["rawPayload"] = payloadRaw
        };
    }

    protected static void Touch(CardTransactionRecord card, string actor)
    {
        card.UpdateDate = DateTime.Now;
        card.LastModifiedBy = actor;
    }

    protected static bool IsFailedState(string value)
    {
        var normalized = (value ?? string.Empty).Trim().ToUpperInvariant();
        return normalized is ReconciliationFieldValues.EMoneyStateFailed
            or ReconciliationFieldValues.EMoneyStateReject
            or ReconciliationFieldValues.EMoneyStateRejected
            or ReconciliationFieldValues.EMoneyStateDeclined;
    }

    protected async Task<bool> HasEMoneyTransactionAsync(
        string customerTransactionId,
        CancellationToken cancellationToken)
    {
        var lookup = await EMoneyTransactionHttpClient.GetByCustomerTransactionIdAsync(customerTransactionId, cancellationToken);
        return lookup.Status == EMoneyTransactionLookupStatus.Found;
    }

    protected static string ResolveOperationIdempotencyKey(ReconciliationOperationPlan plan)
    {
        if (plan?.DerivedFields is null)
        {
            return null;
        }

        return plan.DerivedFields.TryGetValue(ReconciliationDerivedFieldKeys.CurrentOperationId, out var operationId) &&
               !string.IsNullOrWhiteSpace(operationId)
            ? operationId
            : null;
    }

}
