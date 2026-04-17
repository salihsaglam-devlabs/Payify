using System.Text.Json;
using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Domain.Entities.FileIngestion.Schemas;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Flows;

internal static class MscSnapshotBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public static Dictionary<string, List<OperationPayloadItem>> Create(
        MscEvaluationContext context,
        string operationCode,
        string reason,
        params (string Key, object? Value)[] extra)
    {
        var payload = new Dictionary<string, List<OperationPayloadItem>>();
        var latestEmoney = GetLatestEmoneyTransaction(context.EmoneyTransactions);

        AddGroup(payload, operationCode,
            ("correlationKey", context.CorrelationKey),
            ("correlationValue", context.CorrelationValue),
            ("reason", reason),
            ("rootRowId", context.RootRow.Id),
            ("transactionFileId", context.RootRow.FileId),
            ("lineNumber", context.RootRow.LineNumber));

        var detail = DeserializeRootCardDetail(context.RootRow);
        if (detail is null)
        {
            return payload;
        }

        AddGroup(payload, operationCode,
            ("currentTransactionId", detail.OceanTxnGuid),
            ("referenceTransactionId", detail.OceanMainTxnGuid > 0 ? detail.OceanMainTxnGuid : detail.OceanTxnGuid),
            ("responseCode", detail.ResponseCode),
            ("isSettlementReceived", detail.IsTxnSettle == CardMscIsTxnSettle.Settled),
            ("txnEffect", detail.TxnEffect.ToString()),
            ("billingAmount", detail.BillingAmount),
            ("currencyCode", detail.BillingCurrency.ToString()),
            ("cardNo", detail.CardNo),
            ("merchantId", detail.MerchantId),
            ("externalReferenceId", detail.Rrn),
            ("description", detail.TxnDescription),
            ("payifyTransactionId", latestEmoney?.Id));

        foreach (var item in extra)
        {
            AddGroup(payload, operationCode, (item.Key, item.Value));
        }

        return payload;
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

    private static EmoneyCustomerTransactionDto? GetLatestEmoneyTransaction(IReadOnlyList<EmoneyCustomerTransactionDto> transactions)
    {
        return transactions
            .OrderByDescending(x => x.TransactionDate)
            .ThenByDescending(x => x.Id)
            .FirstOrDefault();
    }

    private static CardMscDetail? DeserializeRootCardDetail(IngestionFileLine row)
    {
        if (!string.Equals(row.LineType, "D", StringComparison.OrdinalIgnoreCase) ||
            string.IsNullOrWhiteSpace(row.ParsedContent))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<CardMscDetail>(row.ParsedContent, JsonOptions);
        }
        catch
        {
            return null;
        }
    }
}
