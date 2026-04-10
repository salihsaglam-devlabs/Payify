using System.Text.Json;
using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Domain.Entities.FileIngestion.Schemas;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Flows;

internal static class BkmSnapshotBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const string CurrentCardRowMissingMessage = "Current card row is missing.";

    public static Dictionary<string, List<OperationPayloadItem>> Create(
        BkmEvaluationContext context,
        string operationCode,
        string reason,
        params (string Key, object? Value)[] extra)
    {
        var detail = context.CachedRootDetail ?? DeserializeRootCardDetail(context.RootRow)
            ?? throw new ReconciliationContextException(ApiErrorCode.ReconciliationCurrentCardRowMissing, CurrentCardRowMissingMessage);
        var latestEmoney = context.EmoneyTransactions
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();

        var snapshot = new Dictionary<string, List<OperationPayloadItem>>();

        AddGroup(snapshot, operationCode,
            ("correlationKey", context.CorrelationKey),
            ("correlationValue", context.CorrelationValue),
            ("reason", reason),
            ("rootRowId", context.RootRow.Id),
            ("transactionFileId", context.RootRow.IngestionFileId),
            ("lineNumber", context.RootRow.LineNumber),
            ("currentTransactionId", detail.OceanTxnGuid),
            ("referenceTransactionId", detail.OceanMainTxnGuid > 0 ? detail.OceanMainTxnGuid : detail.OceanTxnGuid),
            ("responseCode", detail.ResponseCode),
            ("isSettlementReceived", detail.IsTxnSettle == CardBkmIsTxnSettle.Settled),
            ("txnEffect", detail.TxnEffect.ToString()),
            ("billingAmount", detail.BillingAmount),
            ("currencyCode", FirstNonEmpty(
                detail.BillingCurrency.ToString(),
                detail.CardHolderBillingCurrency.ToString(),
                detail.OriginalCurrency.ToString())),
            ("cardNo", detail.CardNo),
            ("merchantId", detail.MerchantId),
            ("externalReferenceId", detail.Rrn),
            ("description", detail.TxnDescription),
            ("payifyTransactionId", latestEmoney?.Id));

        foreach (var item in extra)
        {
            AddGroup(snapshot, operationCode, (item.Key, item.Value));
        }

        return snapshot;
    }

    private static void AddGroup(
        Dictionary<string, List<OperationPayloadItem>> payload,
        string group,
        params (string Key, object? Value)[] items)
    {
        if (!payload.TryGetValue(group, out var list))
        {
            list = [];
            payload[group] = list;
        }

        foreach (var item in items)
        {
            list.Add(new OperationPayloadItem
            {
                Key = item.Key,
                Value = item.Value?.ToString()
            });
        }
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        return string.Empty;
    }

    private static CardBkmDetail? DeserializeRootCardDetail(IngestionFileLine row)
    {
        if (!string.Equals(row.RecordType, "D", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(row.ParsedData))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CardBkmDetail>(row.ParsedData, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
