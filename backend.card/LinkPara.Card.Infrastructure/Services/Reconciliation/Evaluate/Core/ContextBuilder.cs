using System.Text.Json;
using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Domain.Entities.FileIngestion.Persistence;
using LinkPara.Card.Domain.Entities.FileIngestion.Schemas;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Integrations.Emoney;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Evaluate.Core;

internal sealed class ContextBuilder : IContextBuilder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private const int MaxEmoneyConcurrency = 8;
    private const int CorrelationLookupBatchSize = 5_000;
    private const string OceanTxnGuidCorrelationKey = "OceanTxnGuid";

    private readonly CardDbContext _dbContext;
    private readonly IEmoneyService _emoneyService;
    private readonly IReconciliationErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;

    public ContextBuilder(
        CardDbContext dbContext,
        IEmoneyService emoneyService,
        IReconciliationErrorMapper errorMapper,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory)
    {
        _dbContext = dbContext;
        _emoneyService = emoneyService;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
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

        var fileIds = rows.Select(x => x.FileId).Distinct().ToArray();
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

        var correlatedGroups = new Dictionary<string, IReadOnlyList<IngestionFileLine>>();
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
                    .Where(x => x.LineType == "D")
                    .Where(x => x.CorrelationKey == group.Key && batch.Contains(x.CorrelationValue))
                    .OrderBy(x => x.LineNumber)
                    .ThenBy(x => x.Id)
                    .ToListAsync(cancellationToken);

                foreach (var rowGroup in rowsByKey.GroupBy(x => $"{x.CorrelationKey}::{x.CorrelationValue}"))
                {
                    correlatedGroups[rowGroup.Key] = rowGroup.ToList();
                }
            }
        }

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

            if (!rootFiles.TryGetValue(row.FileId, out var rootFile))
            {
                throw new ReconciliationFileNotResolvedException(_localizer.Get("Reconciliation.FileNotResolved", row.FileId));
            }

            var emoneyTransactions = new List<EmoneyCustomerTransactionDto>();
            if (string.Equals(correlationKey, OceanTxnGuidCorrelationKey, StringComparison.Ordinal))
            {
                var emoneyFetchResult = await emoneyTasks[correlationValue];
                if (!emoneyFetchResult.IsSuccess)
                {
                    errors?.Add(_errorMapper.Create(
                        code: "EMONEY_LOOKUP_FAILED",
                        message: _localizer.Get("Reconciliation.EmoneyLookupFailed"),
                        step: "EMONEY_LOOKUP",
                        detail: emoneyFetchResult.ErrorMessage,
                        severity: "Error"));
                    continue;
                }

                emoneyTransactions = emoneyFetchResult.Transactions.ToList();
            }

            contexts[row.Id] = BuildTypedContext(rootFile, row, correlationKey, correlationValue, rowGroup, emoneyTransactions);
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
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            return EmoneyFetchResult.Fail(
                _localizer.Get("Reconciliation.CustomerTxnLookupFailed", customerTransactionId, ex.Message));
        }
        finally
        {
            semaphore.Release();
        }
    }

    private EvaluationContext BuildTypedContext(
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
                RootRow = row, RootFile = rootFile, CorrelationKey = correlationKey, CorrelationValue = correlationValue,
                CardDetails = CollectDetails<CardBkmDetail>(correlatedRows, FileType.Card, FileContentType.Bkm),
                ClearingDetails = CollectDetails<ClearingBkmDetail>(correlatedRows, FileType.Clearing, FileContentType.Bkm),
                EmoneyTransactions = emoneyTransactions
            },
            FileContentType.Visa => new VisaEvaluationContext
            {
                RootRow = row, RootFile = rootFile, CorrelationKey = correlationKey, CorrelationValue = correlationValue,
                CardDetails = CollectDetails<CardVisaDetail>(correlatedRows, FileType.Card, FileContentType.Visa),
                ClearingDetails = CollectDetails<ClearingVisaDetail>(correlatedRows, FileType.Clearing, FileContentType.Visa),
                EmoneyTransactions = emoneyTransactions
            },
            FileContentType.Msc => new MscEvaluationContext
            {
                RootRow = row, RootFile = rootFile, CorrelationKey = correlationKey, CorrelationValue = correlationValue,
                CardDetails = CollectDetails<CardMscDetail>(correlatedRows, FileType.Card, FileContentType.Msc),
                ClearingDetails = CollectDetails<ClearingMscDetail>(correlatedRows, FileType.Clearing, FileContentType.Msc),
                EmoneyTransactions = emoneyTransactions
            },
            _ => throw new ReconciliationUnsupportedContentTypeException(_localizer.Get("Reconciliation.UnsupportedContentType", rootFile.ContentType))
        };
    }

    private static List<TDetail> CollectDetails<TDetail>(
        IEnumerable<IngestionFileLine> rows, FileType fileType, FileContentType contentType)
        where TDetail : class
    {
        return rows
            .Where(x => x.IngestionFile.FileType == fileType && x.IngestionFile.ContentType == contentType && string.Equals(x.LineType, "D", StringComparison.OrdinalIgnoreCase))
            .Select(x => Deserialize<TDetail>(x.ParsedContent))
            .Where(x => x is not null)
            .Cast<TDetail>()
            .ToList();
    }

    private static TModel? Deserialize<TModel>(string json) where TModel : class
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try { return JsonSerializer.Deserialize<TModel>(json, JsonOptions); }
        catch { return null; }
    }

    private sealed record EmoneyFetchResult(bool IsSuccess, IReadOnlyCollection<EmoneyCustomerTransactionDto> Transactions, string? ErrorMessage)
    {
        public static EmoneyFetchResult Success(IReadOnlyCollection<EmoneyCustomerTransactionDto> transactions) => new(true, transactions, null);
        public static EmoneyFetchResult Fail(string errorMessage) => new(false, Array.Empty<EmoneyCustomerTransactionDto>(), errorMessage);
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
