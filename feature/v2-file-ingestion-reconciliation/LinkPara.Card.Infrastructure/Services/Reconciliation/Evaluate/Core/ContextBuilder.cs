using LinkPara.Card.Application.Commons.Helpers.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using System.Text.Json;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;
using Microsoft.EntityFrameworkCore;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate;

internal sealed class ContextBuilder : IContextBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const int MaxEmoneyConcurrency = 8;
    private const int CorrelationLookupBatchSize = 5_000;
    private const string OceanTxnGuidCorrelationKey = "OceanTxnGuid";

    private readonly CardDbContext _dbContext;
    private readonly IEmoneyService _emoneyService;

    public ContextBuilder(
        CardDbContext dbContext,
        IEmoneyService emoneyService)
    {
        _dbContext = dbContext;
        _emoneyService = emoneyService;
    }

    public async Task<IReadOnlyDictionary<Guid, EvaluationContext>> BuildManyAsync(
        IReadOnlyList<IngestionFileLine> rows,
        List<ReconciliationErrorDetail>? errors = null,
        CancellationToken cancellationToken = default)
    {
        if (rows.Count == 0)
        {
            return new Dictionary<Guid, EvaluationContext>();
        }

        var fileIds = rows.Select(x => x.IngestionFileId).Distinct().ToArray();
        var rootFiles = await _dbContext.IngestionFiles
            .AsNoTracking()
            .Where(x => fileIds.Contains(x.Id))
            .ToDictionaryAsync(x => x.Id, cancellationToken);

        var validRows = rows
            .Where(x => !string.IsNullOrWhiteSpace(x.CorrelationValue))
            .ToList();

        var correlationPairs = validRows
            .Select(x => new { Key = x.CorrelationKey?.Trim() ?? string.Empty, Value = x.CorrelationValue!.Trim() })
            .Where(x => !string.IsNullOrWhiteSpace(x.Key) && !string.IsNullOrWhiteSpace(x.Value))
            .Distinct()
            .ToList();

        var correlatedRows = new List<IngestionFileLine>();
        foreach (var group in correlationPairs.GroupBy(x => x.Key, x => x.Value, StringComparer.Ordinal))
        {
            var values = group
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            foreach (var batch in Batch(values, CorrelationLookupBatchSize))
            {
                var rowsByKey = await _dbContext.IngestionFileLines
                    .AsNoTracking()
                    .Include(x => x.IngestionFile)
                    .Where(x => x.RecordType == "D")
                    .Where(x => x.CorrelationKey == group.Key && batch.Contains(x.CorrelationValue))
                    .OrderBy(x => x.LineNumber)
                    .ThenBy(x => x.Id)
                    .ToListAsync(cancellationToken);

                correlatedRows.AddRange(rowsByKey);
            }
        }

        var correlatedGroups = correlatedRows
            .GroupBy(x => $"{x.CorrelationKey}::{x.CorrelationValue}")
            .ToDictionary(x => x.Key, x => (IReadOnlyList<IngestionFileLine>)x.ToList());

        var distinctValues = correlationPairs
            .Where(x => string.Equals(x.Key, OceanTxnGuidCorrelationKey, StringComparison.Ordinal))
            .Select(x => x.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var emoneyTasks = new Dictionary<string, Task<EmoneyFetchResult>>(StringComparer.Ordinal);
        using var semaphore = new SemaphoreSlim(Math.Min(MaxEmoneyConcurrency, distinctValues.Length == 0 ? 1 : distinctValues.Length));

        foreach (var value in distinctValues)
        {
            emoneyTasks[value] = FetchEmoneyTransactionsAsync(value, semaphore, cancellationToken);
        }

        await Task.WhenAll(emoneyTasks.Values);

        var contexts = new Dictionary<Guid, EvaluationContext>(validRows.Count);

        foreach (var row in validRows)
        {
            var correlationKey = row.CorrelationKey?.Trim() ?? string.Empty;
            var correlationValue = row.CorrelationValue?.Trim() ?? string.Empty;
            var groupKey = $"{correlationKey}::{correlationValue}";
            correlatedGroups.TryGetValue(groupKey, out var rowGroup);
            rowGroup ??= Array.Empty<IngestionFileLine>();

            if (!rootFiles.TryGetValue(row.IngestionFileId, out var rootFile))
            {
                throw new InvalidOperationException($"IngestionFile '{row.IngestionFileId}' could not be resolved.");
            }

            var emoneyTransactions = new List<EmoneyCustomerTransactionDto>();
            if (string.Equals(correlationKey, OceanTxnGuidCorrelationKey, StringComparison.Ordinal))
            {
                var emoneyFetchResult = await emoneyTasks[correlationValue];
                if (!emoneyFetchResult.IsSuccess)
                {
                    errors?.Add(ReconciliationErrorMapper.Create(
                        code: "EMONEY_LOOKUP_FAILED",
                        message: "Emoney transaction lookup failed. Evaluation skipped for related rows.",
                        step: "EMONEY_LOOKUP",
                        detail: emoneyFetchResult.ErrorMessage,
                        severity: "Error"));
                    continue;
                }

                emoneyTransactions = emoneyFetchResult.Transactions.ToList();
            }

            contexts[row.Id] = BuildTypedContext(
                rootFile,
                row,
                correlationKey,
                correlationValue,
                rowGroup,
                emoneyTransactions);
        }

        return contexts;
    }

    private async Task<EmoneyFetchResult> FetchEmoneyTransactionsAsync(
        string customerTransactionId,
        SemaphoreSlim semaphore,
        CancellationToken cancellationToken)
    {
        await semaphore.WaitAsync(cancellationToken);
        try
        {
            var transactions = await _emoneyService.GetByCustomerTransactionIdAsync(customerTransactionId, cancellationToken);
            return EmoneyFetchResult.Success(transactions);
        }
        catch (EmoneyIntegrationException ex)
        {
            return EmoneyFetchResult.Fail(
                $"customerTransactionId '{customerTransactionId}' lookup failed. {ex.Message}");
        }
        finally
        {
            semaphore.Release();
        }
    }

    private static EvaluationContext BuildTypedContext(
        IngestionFile rootFile,
        IngestionFileLine row,
        string correlationKey,
        string correlationValue,
        IReadOnlyList<IngestionFileLine> correlatedRows,
        IReadOnlyList<EmoneyCustomerTransactionDto> emoneyTransactions)
    {
        return rootFile.ContentType switch
        {
            FileContentType.Bkm => new BkmEvaluationContext
            {
                RootRow = row,
                RootFile = rootFile,
                CorrelationKey = correlationKey,
                CorrelationValue = correlationValue,
                CardDetails = CollectDetails<CardBkmDetail>(correlatedRows, FileType.Card, FileContentType.Bkm),
                ClearingDetails = CollectDetails<ClearingBkmDetail>(correlatedRows, FileType.Clearing, FileContentType.Bkm),
                EmoneyTransactions = emoneyTransactions
            },
            FileContentType.Visa => new VisaEvaluationContext
            {
                RootRow = row,
                RootFile = rootFile,
                CorrelationKey = correlationKey,
                CorrelationValue = correlationValue,
                CardDetails = CollectDetails<CardVisaDetail>(correlatedRows, FileType.Card, FileContentType.Visa),
                ClearingDetails = CollectDetails<ClearingVisaDetail>(correlatedRows, FileType.Clearing, FileContentType.Visa),
                EmoneyTransactions = emoneyTransactions
            },
            FileContentType.Msc => new MscEvaluationContext
            {
                RootRow = row,
                RootFile = rootFile,
                CorrelationKey = correlationKey,
                CorrelationValue = correlationValue,
                CardDetails = CollectDetails<CardMscDetail>(correlatedRows, FileType.Card, FileContentType.Msc),
                ClearingDetails = CollectDetails<ClearingMscDetail>(correlatedRows, FileType.Clearing, FileContentType.Msc),
                EmoneyTransactions = emoneyTransactions
            },
            _ => throw new InvalidOperationException($"Unsupported reconciliation content type '{rootFile.ContentType}'.")
        };
    }

    private static List<TDetail> CollectDetails<TDetail>(
        IEnumerable<IngestionFileLine> rows,
        FileType fileType,
        FileContentType contentType)
        where TDetail : class
    {
        return rows
            .Where(x =>
                x.IngestionFile.FileType == fileType &&
                x.IngestionFile.ContentType == contentType &&
                string.Equals(x.RecordType, "D", StringComparison.OrdinalIgnoreCase))
            .Select(x => Deserialize<TDetail>(x.ParsedData))
            .Where(x => x is not null)
            .Cast<TDetail>()
            .ToList();
    }

    private static TModel? Deserialize<TModel>(string json)
        where TModel : class
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        try
        {
            return JsonSerializer.Deserialize<TModel>(json, JsonOptions);
        }
        catch
        {
            return null;
        }
    }

    private sealed record EmoneyFetchResult(
        bool IsSuccess,
        IReadOnlyCollection<EmoneyCustomerTransactionDto> Transactions,
        string? ErrorMessage)
    {
        public static EmoneyFetchResult Success(IReadOnlyCollection<EmoneyCustomerTransactionDto> transactions)
            => new(true, transactions, null);

        public static EmoneyFetchResult Fail(string errorMessage)
            => new(false, Array.Empty<EmoneyCustomerTransactionDto>(), errorMessage);
    }

    private static IEnumerable<string[]> Batch(string[] source, int batchSize)
    {
        for (var index = 0; index < source.Length; index += batchSize)
        {
            var length = Math.Min(batchSize, source.Length - index);
            var batch = new string[length];
            Array.Copy(source, index, batch, 0, length);
            yield return batch;
        }
    }

}
