using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LinkPara.Card.Application.Commons.Constants;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Models.FileIngestion;
using LinkPara.Card.Application.Commons.Models.Logging;
using LinkPara.Card.Application.Commons.Models.Reconciliation;
using LinkPara.Card.Domain.Constants;
using LinkPara.Card.Domain.Entities.FileIngestion;
using LinkPara.Card.Domain.Entities.Reconciliation;
using LinkPara.Card.Domain.Enums;
using LinkPara.Card.Infrastructure.Persistence;
using LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration.CardFlow;
using LinkPara.SharedModels.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Reconciliation.Orchestration;

// Core orchestration flow: ingestion-triggered reconciliation, operation creation, state transitions and audit persistence.
public partial class ReconciliationService : IReconciliationService
{
    private readonly CardDbContext _dbContext;
    private readonly IEMoneyService _eMoneyService;
    private readonly IReconciliationAlarmService _alarmService;
    private readonly FileIngestionSettings _settings;
    private readonly FileParsingRulesOptions _fileParsingRules;
    private readonly IBulkServiceOperationLogPublisher _bulkLogPublisher;
    private readonly ILogger<ReconciliationService> _logger;

    public ReconciliationService(
        CardDbContext dbContext,
        IEMoneyService eMoneyService,
        IReconciliationAlarmService alarmService,
        IOptions<FileIngestionSettings> options,
        IOptions<FileParsingRulesOptions> fileParsingRulesOptions,
        IBulkServiceOperationLogPublisher bulkLogPublisher,
        ILogger<ReconciliationService> logger)
    {
        _dbContext = dbContext;
        _eMoneyService = eMoneyService;
        _alarmService = alarmService;
        _settings = options.Value;
        _fileParsingRules = fileParsingRulesOptions.Value ?? new FileParsingRulesOptions();
        _bulkLogPublisher = bulkLogPublisher;
        _logger = logger;
    }

    public async Task<ReconciliationProcessSummary> ProcessImportedFileAsync(
        Guid importedFileId,
        string actor,
        ReconciliationSummaryOptions options = null,
        CancellationToken cancellationToken = default)
    {
        return await RunWithBulkLogAsync(
            endpointName: nameof(ProcessImportedFileAsync),
            actor: actor,
            metadata: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["importedFileId"] = importedFileId.ToString()
            },
            run: async () =>
            {
                var cards = await _dbContext.CardTransactionRecords
                    .AsNoTracking()
                    .Where(x => x.ImportedFileRow.ImportedFileId == importedFileId)
                    .OrderBy(x => x.CreateDate)
                    .ThenBy(x => x.Id)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Reconciliation process started. ImportedFileId={ImportedFileId}, CardCount={CardCount}", importedFileId, cards.Count);

                var summary = await ExecuteInDbTransactionAsync(async ct =>
                {
                    var result = await ProcessCardsAsync(cards, actor, ct);
                    await _dbContext.SaveChangesAsync(ct);
                    return result;
                }, cancellationToken);

                options ??= new ReconciliationSummaryOptions();
                var finalized = await FinalizeSummaryAsync(summary, options, cancellationToken);
                _logger.LogInformation("Reconciliation process completed. ImportedFileId={ImportedFileId}, ReconciledCards={ReconciledCards}, Operations={Operations}, ManualReviews={ManualReviews}",
                    importedFileId,
                    finalized.ReconciledCards,
                    finalized.ReconciliationOperations,
                    finalized.ReconciliationManualReviewItems);
                return finalized;
            },
            cancellationToken);
    }

    public async Task<ReconciliationProcessSummary> RegenerateOperationsAsync(
        string actor,
        int take = 1000,
        Guid? importedFileId = null,
        int? lookbackDays = null,
        ReconciliationSummaryOptions options = null,
        CancellationToken cancellationToken = default)
    {
        var effectiveLookbackDays = lookbackDays.GetValueOrDefault() > 0
            ? lookbackDays!.Value
            : (_settings.ReconciliationProcessing?.RegenerateCandidateLookbackDays > 0
                ? _settings.ReconciliationProcessing.RegenerateCandidateLookbackDays
                : 2);

        return await RunWithBulkLogAsync(
            endpointName: nameof(RegenerateOperationsAsync),
            actor: actor,
            metadata: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["take"] = take.ToString(CultureInfo.InvariantCulture),
                ["importedFileId"] = importedFileId?.ToString() ?? string.Empty,
                ["lookbackDays"] = effectiveLookbackDays.ToString(CultureInfo.InvariantCulture)
            },
            run: async () =>
            {
                if (take <= 0)
                {
                    take = 1000;
                }

                var recentClearingByCorrelationKey = BuildRecentClearingByCorrelationKeyQuery(importedFileId, effectiveLookbackDays);
                var candidateQuery = _dbContext.CardTransactionRecords
                    .AsNoTracking()
                    .Where(card =>
                        card.ReconciliationState != CardReconciliationState.ReconcileCompleted ||
                        (!string.IsNullOrWhiteSpace(card.CorrelationKey) &&
                         recentClearingByCorrelationKey.Any(clearing =>
                             clearing.CorrelationKey == card.CorrelationKey &&
                             (!card.ReconciliationStateUpdatedAt.HasValue ||
                              clearing.LatestClearingCreateDate > card.ReconciliationStateUpdatedAt.Value))))
                    .OrderBy(card => card.CreateDate)
                    .ThenBy(card => card.Id)
                    .Take(take);

                var cards = await candidateQuery.ToListAsync(cancellationToken);
                if (cards.Count == 0)
                {
                    _logger.LogInformation("Regenerate operations skipped. No candidate cards. Take={Take}, ImportedFileId={ImportedFileId}, LookbackDays={LookbackDays}",
                        take,
                        importedFileId,
                        effectiveLookbackDays);
                    return new ReconciliationProcessSummary();
                }

                _logger.LogInformation("Regenerate operations started. CardCount={CardCount}, Take={Take}, ImportedFileId={ImportedFileId}, LookbackDays={LookbackDays}",
                    cards.Count,
                    take,
                    importedFileId,
                    effectiveLookbackDays);

                var summary = await ExecuteInDbTransactionAsync(async ct =>
                {
                    var result = await ProcessCardsAsync(cards, actor, ct);
                    await _dbContext.SaveChangesAsync(ct);
                    return result;
                }, cancellationToken);

                options ??= new ReconciliationSummaryOptions();
                var finalized = await FinalizeSummaryAsync(summary, options, cancellationToken);
                _logger.LogInformation("Regenerate operations completed. ReconciledCards={ReconciledCards}, Operations={Operations}, ManualReviews={ManualReviews}",
                    finalized.ReconciledCards,
                    finalized.ReconciliationOperations,
                    finalized.ReconciliationManualReviewItems);
                return finalized;
            },
            cancellationToken);
    }

    private async Task<ReconciliationProcessSummary> ProcessCardsAsync(
        IReadOnlyCollection<CardTransactionRecord> cards,
        string actor,
        CancellationToken cancellationToken)
    {
        if (cards.Count == 0)
        {
            return new ReconciliationProcessSummary();
        }

        var executionGroupId = Guid.NewGuid();
        var generatedRunIds = new HashSet<Guid>();
        var operationCount = 0;
        var manualReviewCount = 0;

        var duplicateRules = BuildDuplicateRules(cards);
        var correlationKeyByCardId = cards.ToDictionary(
            x => x.Id,
            x => string.IsNullOrWhiteSpace(x.CorrelationKey)
                ? $"{CorrelationKeyValues.CardPrefix}{CorrelationKeyValues.SegmentSeparator}{x.Id:N}"
                : CardFlowText.Normalize(x.CorrelationKey));
        var selectedClearingByKey = await LoadSelectedClearingByCorrelationKeyAsync(correlationKeyByCardId.Values.Distinct(StringComparer.Ordinal).ToArray(), cancellationToken);
        var expireControlPByCardId = await LoadExpireControlStatPByCardIdAsync(cards, cancellationToken);
        var prepaidLookupByCardId = await LoadPrepaidLookupByCardIdAsync(cards, cancellationToken);
        var providerFallbackByCardId = await LoadProviderFallbackByCardIdAsync(cards, cancellationToken);

        foreach (var card in cards)
        {
            duplicateRules.TryGetValue(card.Id, out var duplicateRule);
            if (duplicateRule is { ShouldRaiseAlarm: true })
            {
                await RaiseDuplicateAlarmAsync(card, duplicateRule, cancellationToken);
            }

            var eval = EvaluateSingleCard(
                card,
                duplicateRules,
                correlationKeyByCardId,
                selectedClearingByKey,
                expireControlPByCardId,
                prepaidLookupByCardId,
                providerFallbackByCardId);
            var operationsToCreate = eval.Flow.Operations;
            var decision = ResolveDecision(operationsToCreate, eval.ClearingRecordId, eval.ClearingControlStat, duplicateRule);

            var now = DateTime.Now;
            var runId = operationsToCreate.Count > 0 ? Guid.NewGuid() : (Guid?)null;

            await CloseOpenRunsForCardAsync(card.Id, actor, now, cancellationToken);

            IReadOnlyCollection<ReconciliationOperation> created = [];
            if (operationsToCreate.Count > 0 && runId.HasValue)
            {
                created = CreateOperations(card, eval.ClearingRecordId, runId.Value, executionGroupId, operationsToCreate, actor, now);
            }

            var (plannedState, plannedReason) = ResolvePlannedCardState(operationsToCreate, decision.DecisionCode);
            await UpdateCardReconciliationStateAsync(card.Id, plannedState, plannedReason, runId, executionGroupId, actor, now, cancellationToken);
            CreateEvaluationRecord(card.Id, runId, executionGroupId, eval.ClearingRecordId, operationsToCreate, decision, actor, now);
            operationCount += created.Count;
            if (runId.HasValue)
            {
                generatedRunIds.Add(runId.Value);
            }

            foreach (var operation in created.Where(x => x.Mode == ReconciliationOperationMode.Manual))
            {
                _dbContext.ReconciliationManualReviewItems.Add(new ReconciliationManualReviewItem
                {
                    Id = Guid.NewGuid(),
                    ReconciliationOperationId = operation.Id,
                    RunId = runId!.Value,
                    ExecutionGroupId = executionGroupId,
                    ReviewStatus = ManualReviewStatus.Pending,
                    ReviewNote = ReconciliationTextValues.PendingManualDecision,
                    CreateDate = now,
                    CreatedBy = actor,
                    UpdateDate = now,
                    LastModifiedBy = actor,
                    RecordStatus = RecordStatus.Active
                });
                manualReviewCount++;
            }

            if (created.Any(x => x.Mode == ReconciliationOperationMode.Manual))
            {
                await RaiseManualReviewAlarmIfNeededAsync(card, operationsToCreate, cancellationToken);
            }
        }

        return new ReconciliationProcessSummary
        {
            ReconciledCards = cards.Count,
            ReconciliationOperations = operationCount,
            ReconciliationManualReviewItems = manualReviewCount,
            Details = new ReconciliationProcessSummaryDetails
            {
                ExecutionGroupId = executionGroupId,
                GeneratedRunIds = generatedRunIds.ToArray(),
                InMemoryCounts = new ReconciliationSummaryCounts
                {
                    ReconciledCards = cards.Count,
                    ReconciliationOperations = operationCount,
                    ReconciliationManualReviewItems = manualReviewCount
                }
            }
        };
    }

    private List<ReconciliationOperation> CreateOperations(
        CardTransactionRecord card,
        Guid? clearingRecordId,
        Guid runId,
        Guid executionGroupId,
        IReadOnlyList<FlowOperation> operationDefs,
        string actor,
        DateTime now)
    {
        var list = new List<ReconciliationOperation>(operationDefs.Count);
        var orderedDefinitions = operationDefs.OrderBy(x => x.Index).ToArray();
        for (var operationDefinitionIndex = 0; operationDefinitionIndex < orderedDefinitions.Length; operationDefinitionIndex++)
        {
            var operationDefinition = orderedDefinitions[operationDefinitionIndex];
            var operationIndex = operationDefinition.Index;
            var dependsOnIndex = operationDefinitionIndex == 0 ? (int?)null : orderedDefinitions[operationDefinitionIndex - 1].Index;
            var operationCode = operationDefinition.Code.ToOperationCode();
            var operationHandlerName = $"{operationCode}OperationHandler";
            var payload = JsonSerializer.Serialize(new
            {
                operationCode,
                operationHandlerName,
                operationMode = operationDefinition.Mode.ToString(),
                operationIndex,
                dependsOnIndex,
                operationTrigger = operationDefinition.ReasonText,
                metadata = BuildOperationPayloadMetadata(card, clearingRecordId, runId, executionGroupId, operationDefinition)
            });
            var idempotencyKey = $"{card.Id:N}:{runId:N}:{operationCode}:{operationIndex}";

            var entity = new ReconciliationOperation
            {
                Id = Guid.NewGuid(),
                CardTransactionRecordId = card.Id,
                ClearingRecordId = clearingRecordId,
                RunId = runId,
                ExecutionGroupId = executionGroupId,
                OperationIndex = operationIndex,
                DependsOnIndex = dependsOnIndex,
                IsApprovalGate = operationDefinition.Mode == ReconciliationOperationMode.Manual,
                OperationCode = operationCode,
                Mode = operationDefinition.Mode,
                Status = dependsOnIndex.HasValue ? ReconciliationOperationStatus.Blocked : ReconciliationOperationStatus.Pending,
                IdempotencyKey = idempotencyKey,
                Fingerprint = BuildFingerprint(card, operationCode),
                Payload = payload,
                CreateDate = now,
                CreatedBy = actor,
                UpdateDate = now,
                LastModifiedBy = actor,
                RecordStatus = RecordStatus.Active
            };

            _dbContext.ReconciliationOperations.Add(entity);
            list.Add(entity);
        }

        return list;
    }

    private async Task CloseOpenRunsForCardAsync(Guid cardId, string actor, DateTime now, CancellationToken cancellationToken)
    {
        var openOperations = await _dbContext.ReconciliationOperations
            .AsTracking()
            .Where(x => x.CardTransactionRecordId == cardId &&
                        (x.Status == ReconciliationOperationStatus.Pending ||
                         x.Status == ReconciliationOperationStatus.Blocked ||
                         x.Status == ReconciliationOperationStatus.Processing))
            .ToListAsync(cancellationToken);
        if (openOperations.Count == 0)
        {
            return;
        }

        foreach (var op in openOperations)
        {
            op.Status = ReconciliationOperationStatus.Skipped;
            op.ErrorCode = ReconciliationErrorCodes.ObsoleteByReevaluation;
            op.ErrorText = "Operation closed by re-evaluation.";
            op.CompletedAt = now;
            op.UpdateDate = now;
            op.LastModifiedBy = actor;
            await CreateExecutionRecordAsync(
                op,
                ReconciliationOperationExecutionOutcome.Skipped,
                actor,
                ReconciliationErrorCodes.ObsoleteByReevaluation,
                "Operation closed by re-evaluation.");
        }

        var operationIds = openOperations.Select(x => x.Id).ToArray();
        var pendingManuals = await _dbContext.ReconciliationManualReviewItems
            .AsTracking()
            .Where(x => operationIds.Contains(x.ReconciliationOperationId) && x.ReviewStatus == ManualReviewStatus.Pending)
            .ToListAsync(cancellationToken);
        foreach (var manual in pendingManuals)
        {
            manual.ReviewStatus = ManualReviewStatus.Rejected;
            manual.ReviewNote = "Closed by re-evaluation.";
            manual.ReviewedBy = actor;
            manual.ReviewedAt = now;
            manual.UpdateDate = now;
            manual.LastModifiedBy = actor;
        }
    }

    private static string BuildFingerprint(CardTransactionRecord card, string operationCode)
    {
        var referenceTxnGuid = string.IsNullOrWhiteSpace(card.OceanMainTxnGuid)
            ? card.OceanTxnGuid
            : card.OceanMainTxnGuid;
        var payload = string.Join("|",
            operationCode ?? string.Empty,
            CardFlowText.Normalize(card.OceanTxnGuid),
            CardFlowText.Normalize(referenceTxnGuid),
            NormalizeAmount(ResolveAccountingAmountForCard(card)),
            CardFlowText.Normalize(ResolveAccountingCurrencyForCard(card)),
            CardFlowText.Normalize(card.TxnEffect));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(payload)));
    }

    private async Task<ReconciliationProcessSummary> FinalizeSummaryAsync(
        ReconciliationProcessSummary summary,
        ReconciliationSummaryOptions options,
        CancellationToken cancellationToken)
    {
        if (options.IncludeDetails != true)
        {
            summary.Details = null;
            return summary;
        }

        summary.Details ??= new ReconciliationProcessSummaryDetails
        {
            InMemoryCounts = new ReconciliationSummaryCounts
            {
                ReconciledCards = summary.ReconciledCards,
                ReconciliationOperations = summary.ReconciliationOperations,
                ReconciliationManualReviewItems = summary.ReconciliationManualReviewItems
            }
        };

        if (options.VerifyWithDbAudit)
        {
            summary.Details.DbAuditCounts = await BuildDbAuditCountsAsync(
                summary.Details.ExecutionGroupId,
                summary.Details.GeneratedRunIds,
                cancellationToken);
        }

        return summary;
    }

    private async Task<ReconciliationSummaryCounts> BuildDbAuditCountsAsync(
        Guid? executionGroupId,
        IReadOnlyCollection<Guid> generatedRunIds,
        CancellationToken cancellationToken)
    {
        if (!executionGroupId.HasValue && (generatedRunIds is null || generatedRunIds.Count == 0))
        {
            return new ReconciliationSummaryCounts();
        }

        IQueryable<ReconciliationEvaluation> evaluationQuery = _dbContext.ReconciliationEvaluations.AsNoTracking();
        IQueryable<ReconciliationOperation> operationQuery = _dbContext.ReconciliationOperations.AsNoTracking();
        IQueryable<ReconciliationManualReviewItem> manualQuery = _dbContext.ReconciliationManualReviewItems.AsNoTracking();

        if (executionGroupId.HasValue)
        {
            evaluationQuery = evaluationQuery.Where(x => x.ExecutionGroupId == executionGroupId.Value);
            operationQuery = operationQuery.Where(x => x.ExecutionGroupId == executionGroupId.Value);
            manualQuery = manualQuery.Where(x => x.ExecutionGroupId == executionGroupId.Value);
        }
        else
        {
            var runIds = generatedRunIds.Distinct().ToArray();
            evaluationQuery = evaluationQuery.Where(x => x.RunId.HasValue && runIds.Contains(x.RunId.Value));
            operationQuery = operationQuery.Where(x => runIds.Contains(x.RunId));
            manualQuery = manualQuery.Where(x => runIds.Contains(x.RunId));
        }

        var reconciledCards = await evaluationQuery
            .Select(x => x.CardTransactionRecordId)
            .Distinct()
            .CountAsync(cancellationToken);

        var operations = await operationQuery.CountAsync(cancellationToken);

        var manualReviews = await manualQuery.CountAsync(cancellationToken);

        return new ReconciliationSummaryCounts
        {
            ReconciledCards = reconciledCards,
            ReconciliationOperations = operations,
            ReconciliationManualReviewItems = manualReviews
        };
    }

    private async Task<T> ExecuteInDbTransactionAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken)
    {
        var strategy = _dbContext.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var result = await operation(cancellationToken);
                await tx.CommitAsync(cancellationToken);
                return result;
            }
            catch
            {
                await tx.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }

    private static EvaluationResult EvaluateSingleCard(
        CardTransactionRecord card,
        Dictionary<Guid, DuplicateRule> duplicateRules,
        Dictionary<Guid, string> correlationKeyByCardId,
        Dictionary<string, SelectedClearingInfo> selectedClearingByKey,
        Dictionary<Guid, bool> expireControlPByCardId,
        Dictionary<Guid, PrepaidLookupInfo> prepaidLookupByCardId,
        Dictionary<Guid, ClearingProvider> providerFallbackByCardId)
    {
        var correlationKey = correlationKeyByCardId[card.Id];
        selectedClearingByKey.TryGetValue(correlationKey, out var selectedClearingInfo);

        if (duplicateRules.TryGetValue(card.Id, out var duplicateRule) && !duplicateRule.ShouldProcess)
        {
            return new EvaluationResult
            {
                ClearingRecordId = selectedClearingInfo?.Id,
                ClearingControlStat = selectedClearingInfo?.ControlStat,
                Flow = new CardFlowResult
                {
                    CardTransactionRecordId = card.Id,
                    ClearingRecordId = selectedClearingInfo?.Id
                }
            };
        }

        var hasExpireControlP = expireControlPByCardId.TryGetValue(card.Id, out var pExists) && pExists;
        var prepaidLookup = prepaidLookupByCardId.TryGetValue(card.Id, out var info) ? info : PrepaidLookupInfo.Unknown();
        var provider = selectedClearingInfo?.Provider;
        if (provider is null && providerFallbackByCardId.TryGetValue(card.Id, out var fallback))
        {
            provider = fallback;
        }

        return new EvaluationResult
        {
            ClearingRecordId = selectedClearingInfo?.Id,
            ClearingControlStat = selectedClearingInfo?.ControlStat,
            Flow = CardFlowEvaluator.EvaluateByProvider(card, prepaidLookup, hasExpireControlP, provider)
        };
    }

    private async Task<Dictionary<Guid, ClearingProvider>> LoadProviderFallbackByCardIdAsync(
        IReadOnlyCollection<CardTransactionRecord> cards,
        CancellationToken cancellationToken)
    {
        if (cards.Count == 0) return new Dictionary<Guid, ClearingProvider>();
        var cardIds = cards.Select(x => x.Id).ToArray();
        var cardRows = await (
            from c in _dbContext.CardTransactionRecords.AsNoTracking()
            join r in _dbContext.ImportedFileRows.AsNoTracking() on c.ImportedFileRowId equals r.Id
            where cardIds.Contains(c.Id)
            select new { c.Id, r.ImportedFileId, r.LineNo, RawLineLength = r.RawLine == null ? 0 : r.RawLine.Length })
            .ToListAsync(cancellationToken);
        var firstRowByFile = cardRows.GroupBy(x => x.ImportedFileId).ToDictionary(x => x.Key, x => x.OrderBy(v => v.LineNo).First());
        var providerByFile = firstRowByFile.ToDictionary(x => x.Key, x => InferProviderFromFirstDetailRowLength(x.Value.RawLineLength, _fileParsingRules));
        return cardRows.ToDictionary(x => x.Id, x => providerByFile.TryGetValue(x.ImportedFileId, out var p) ? p : ClearingProvider.Unknown);
    }

    private static ClearingProvider InferProviderFromFirstDetailRowLength(int firstDetailRowLength, FileParsingRulesOptions fileParsingRules)
    {
        if (firstDetailRowLength <= 0) return ClearingProvider.Unknown;
        if (fileParsingRules?.Files is not null
            && fileParsingRules.Files.TryGetValue(FileParsingRuleKeys.CardTransactions, out var cardRule)
            && cardRule.DetailLength > 0
            && cardRule.DetailLength == firstDetailRowLength)
        {
            return ClearingProvider.Bkm;
        }
        return ClearingProvider.Unknown;
    }

    private async Task<Dictionary<Guid, PrepaidLookupInfo>> LoadPrepaidLookupByCardIdAsync(
        IReadOnlyCollection<CardTransactionRecord> cards,
        CancellationToken cancellationToken)
    {
        var lookbackDays = _settings.ReconciliationProcessing?.PayifyLookupLookbackDays > 0
            ? _settings.ReconciliationProcessing.PayifyLookupLookbackDays
            : 20;
        var earliestTransactionDate = DateTime.Now.AddDays(-lookbackDays);

        var normalizedOceanTransactionIds = cards.Select(x => CardFlowText.Normalize(x.OceanTxnGuid)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray();
        var lookupById = new ConcurrentDictionary<string, EMoneyTransactionLookupResult>(StringComparer.Ordinal);
        var degreeOfParallelism = ResolveEvaluationDegreeOfParallelism();
        await Parallel.ForEachAsync(normalizedOceanTransactionIds, new ParallelOptions { CancellationToken = cancellationToken, MaxDegreeOfParallelism = degreeOfParallelism }, async (normalizedOceanTransactionId, operationCancellationToken) =>
        {
            lookupById[normalizedOceanTransactionId] = await _eMoneyService.GetByCustomerTransactionIdAsync(normalizedOceanTransactionId, operationCancellationToken);
        });

        var result = new Dictionary<Guid, PrepaidLookupInfo>(cards.Count);
        foreach (var card in cards)
        {
            var normalizedOceanTransactionId = CardFlowText.Normalize(card.OceanTxnGuid);
            if (string.IsNullOrWhiteSpace(normalizedOceanTransactionId))
            {
                result[card.Id] = new PrepaidLookupInfo { Status = PrepaidTransactionStatus.Missing };
                continue;
            }
            if (!lookupById.TryGetValue(normalizedOceanTransactionId, out var lookup))
            {
                result[card.Id] = PrepaidLookupInfo.Unknown();
                continue;
            }

            if (lookup.Status == EMoneyTransactionLookupStatus.Found &&
                lookup.TransactionDate.HasValue &&
                lookup.TransactionDate.Value < earliestTransactionDate)
            {
                result[card.Id] = new PrepaidLookupInfo { Status = PrepaidTransactionStatus.Missing };
                continue;
            }

            result[card.Id] = new PrepaidLookupInfo
            {
                Status = PrepaidLookupResolver.ResolvePrepaidStatus(lookup),
                IsCancelled = PrepaidLookupResolver.IsCancelledState(lookup.TransactionState),
                Amount = lookup.Amount
            };
        }
        return result;
    }

    private async Task<Dictionary<string, SelectedClearingInfo>> LoadSelectedClearingByCorrelationKeyAsync(string[] correlationKeys, CancellationToken cancellationToken)
    {
        if (correlationKeys.Length == 0) return new Dictionary<string, SelectedClearingInfo>(StringComparer.Ordinal);
        var baseQuery = _dbContext.ClearingRecords
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.CorrelationKey) && correlationKeys.Contains(x.CorrelationKey));

        var latestByKey = baseQuery
            .GroupBy(x => x.CorrelationKey!)
            .Select(g => new
            {
                CorrelationKey = g.Key,
                LatestCreateDate = g.Max(x => x.CreateDate)
            });

        var latestClearings = await (
            from clearing in baseQuery
            join latest in latestByKey
                on new { CorrelationKey = clearing.CorrelationKey!, clearing.CreateDate }
                equals new { latest.CorrelationKey, CreateDate = latest.LatestCreateDate }
            select new { clearing.Id, CorrelationKey = clearing.CorrelationKey!, clearing.CreateDate, clearing.Provider, clearing.ControlStat })
            .ToListAsync(cancellationToken);

        return latestClearings
            .GroupBy(x => x.CorrelationKey, StringComparer.Ordinal)
            .ToDictionary(
            x => x.Key,
            x =>
            {
                var selected = x.OrderByDescending(v => v.CreateDate).ThenByDescending(v => v.Id).First();
                return new SelectedClearingInfo(selected.Id, selected.Provider, selected.ControlStat);
            }, StringComparer.Ordinal);
    }

    private async Task<Dictionary<Guid, bool>> LoadExpireControlStatPByCardIdAsync(IReadOnlyCollection<CardTransactionRecord> cards, CancellationToken cancellationToken)
    {
        var expireCards = cards.Where(x => CardFlowText.Normalize(x.TxnStat) == ReconciliationFieldValues.TxnStatExpire).ToList();
        if (expireCards.Count == 0) return new Dictionary<Guid, bool>();
        var cardNos = expireCards.Select(x => CardFlowText.Normalize(x.CardNo)).Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.Ordinal).ToArray();
        if (cardNos.Length == 0) return expireCards.ToDictionary(x => x.Id, _ => false);

        var lookbackDays = _settings.ReconciliationProcessing?.ExpireControlStatPLookbackDays > 0 ? _settings.ReconciliationProcessing.ExpireControlStatPLookbackDays : 20;
        var clearingRows = await _dbContext.ClearingRecords
            .AsNoTracking()
            .Where(x => x.ControlStat == ReconciliationFieldValues.ClearingControlStatPending)
            .Where(x => cardNos.Contains(x.CardNo!))
            .Where(x => x.CreateDate >= DateTime.Now.AddDays(-lookbackDays))
            .Select(x => new { x.Rrn, x.CardNo, x.ProvisionCode, x.Arn, Mcc = x.MccCode, Amount = NormalizeAmount(x.SourceAmount), Currency = x.SourceCurrency })
            .ToListAsync(cancellationToken);

        var signatures = clearingRows.Select(row => string.Join("|", row.Rrn ?? "", row.CardNo ?? "", row.ProvisionCode ?? "", row.Arn ?? "", row.Mcc ?? "", row.Amount ?? "", row.Currency ?? "")).ToHashSet(StringComparer.Ordinal);
        return expireCards.ToDictionary(card => card.Id, card => signatures.Contains(string.Join("|", CardFlowText.Normalize(card.Rrn), CardFlowText.Normalize(card.CardNo), CardFlowText.Normalize(card.ProvisionCode), CardFlowText.Normalize(card.Arn), CardFlowText.Normalize(card.Mcc), NormalizeAmount(ResolveAccountingAmountForCard(card)), CardFlowText.Normalize(ResolveAccountingCurrencyForCard(card)))));
    }

    private async Task RaiseManualReviewAlarmIfNeededAsync(
        CardTransactionRecord card,
        IReadOnlyList<FlowOperation> operations,
        CancellationToken cancellationToken)
    {
        var manualOperation = operations
            .Where(x => x.Mode == ReconciliationOperationMode.Manual)
            .OrderBy(x => x.Index)
            .FirstOrDefault();
        if (manualOperation is null)
        {
            return;
        }

        var payload = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["cardTransactionId"] = card.Id.ToString(),
            ["oceanTxnGuid"] = card.OceanTxnGuid ?? string.Empty,
            ["operationCode"] = manualOperation.Code.ToOperationCode(),
            ["operationReasonCode"] = manualOperation.ReasonCode,
            ["operationReasonText"] = manualOperation.ReasonText
        };

        await RaiseAlarmSafeAsync(
            ReconciliationAlarmCodes.ReconciliationManualReview,
            "Manual reconciliation review required.",
            payload,
            cancellationToken);
    }

    private async Task RaiseDuplicateAlarmAsync(
        CardTransactionRecord card,
        DuplicateRule duplicateRule,
        CancellationToken cancellationToken)
    {
        var alarmCode = duplicateRule.DuplicateType == CardDuplicateType.ConflictingSignature
            ? ReconciliationAlarmCodes.ReconciliationDuplicateConflictingSignature
            : ReconciliationAlarmCodes.ReconciliationDuplicateSameSignature;
        var summary = duplicateRule.DuplicateType == CardDuplicateType.ConflictingSignature
            ? "Duplicate rule: conflicting duplicate records detected."
            : "Duplicate rule: same-signature duplicate records detected.";

        await RaiseAlarmSafeAsync(
            alarmCode,
            summary,
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["cardTransactionId"] = card.Id.ToString(),
                ["oceanTxnGuid"] = card.OceanTxnGuid ?? string.Empty,
                ["duplicateGroupKey"] = duplicateRule.DuplicateGroupKey
            },
            cancellationToken);
    }

    private async Task RaiseAlarmSafeAsync(
        string alarmCode,
        string summary,
        IReadOnlyDictionary<string, string> metadata,
        CancellationToken cancellationToken)
    {
        try
        {
            await _alarmService.RaiseAsync(alarmCode, summary, metadata, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Reconciliation alarm raise failed. AlarmCode={AlarmCode}", alarmCode);
        }
    }

    private async Task<ReconciliationProcessSummary> RunWithBulkLogAsync(
        string endpointName,
        string actor,
        Dictionary<string, string> metadata,
        Func<Task<ReconciliationProcessSummary>> run,
        CancellationToken cancellationToken)
    {
        var startedAt = DateTime.Now;
        var steps = new List<string>();
        var success = false;
        ReconciliationProcessSummary summary = null;
        try
        {
            summary = await run();
            success = true;
            steps.Add($"ReconciledCards={summary?.ReconciledCards ?? 0}");
            steps.Add($"Operations={summary?.ReconciliationOperations ?? 0}");
            steps.Add($"ManualReviews={summary?.ReconciliationManualReviewItems ?? 0}");
            return summary;
        }
        catch (Exception ex)
        {
            steps.Add($"Exception={ex.Message}");
            _logger.LogError(ex, "Reconciliation endpoint failed. Endpoint={EndpointName}", endpointName);
            throw;
        }
        finally
        {
            var endedAt = DateTime.Now;
            await _bulkLogPublisher.PublishAsync(new BulkServiceOperationLog
            {
                ServiceName = nameof(ReconciliationService),
                EndpointName = endpointName,
                Actor = string.IsNullOrWhiteSpace(actor) ? AuditUsers.CardFileIngestion : actor,
                StartedAt = startedAt,
                EndedAt = endedAt,
                DurationMs = (long)(endedAt - startedAt).TotalMilliseconds,
                IsSuccess = success,
                Summary = success ? "Reconciliation completed." : "Reconciliation failed.",
                Metadata = metadata ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase),
                Logs = steps
            }, cancellationToken);
        }
    }

    private async Task UpdateCardReconciliationStateAsync(
        Guid cardId,
        CardReconciliationState state,
        string reason,
        Guid? runId,
        Guid? executionGroupId,
        string actor,
        DateTime now,
        CancellationToken cancellationToken)
    {
        await _dbContext.CardTransactionRecords
            .Where(x => x.Id == cardId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ReconciliationState, state)
                .SetProperty(x => x.ReconciliationStateReason, reason)
                .SetProperty(x => x.LastReconciliationRunId, runId)
                .SetProperty(x => x.LastReconciliationExecutionGroupId, executionGroupId)
                .SetProperty(x => x.ReconciliationStateUpdatedAt, now)
                .SetProperty(x => x.UpdateDate, now)
                .SetProperty(x => x.LastModifiedBy, actor), cancellationToken);
    }

    private void CreateEvaluationRecord(
        Guid cardId,
        Guid? runId,
        Guid executionGroupId,
        Guid? clearingRecordId,
        IReadOnlyCollection<FlowOperation> operations,
        ReconciliationDecision decision,
        string actor,
        DateTime now)
    {
        _dbContext.ReconciliationEvaluations.Add(new ReconciliationEvaluation
        {
            Id = Guid.NewGuid(),
            CardTransactionRecordId = cardId,
            RunId = runId,
            ExecutionGroupId = executionGroupId,
            DecisionType = decision.DecisionType,
            DecisionCode = decision.DecisionCode,
            DecisionReason = decision.DecisionReason,
            HasClearing = clearingRecordId.HasValue,
            ClearingRecordId = clearingRecordId,
            PlannedOperationCount = operations.Count,
            PlannedOperationCodes = operations.Count == 0
                ? string.Empty
                : string.Join(",", operations
                    .OrderBy(x => x.Index)
                    .Select(x => x.Code.ToOperationCode())),
            CreateDate = now,
            CreatedBy = actor,
            UpdateDate = now,
            LastModifiedBy = actor,
            RecordStatus = RecordStatus.Active
        });
    }

    private async Task CreateExecutionRecordAsync(
        ReconciliationOperation operation,
        ReconciliationOperationExecutionOutcome outcome,
        string actor,
        string errorCode = null,
        string errorText = null)
    {
        var maxPersistedAttemptNo = await _dbContext.ReconciliationOperationExecutions
            .AsNoTracking()
            .Where(x => x.ReconciliationOperationId == operation.Id)
            .Select(x => (int?)x.AttemptNo)
            .MaxAsync() ?? 0;

        var maxTrackedAttemptNo = _dbContext.ChangeTracker.Entries<ReconciliationOperationExecution>()
            .Where(x => x.Entity.ReconciliationOperationId == operation.Id &&
                        x.State is EntityState.Added or EntityState.Modified or EntityState.Unchanged)
            .Select(x => (int?)x.Entity.AttemptNo)
            .DefaultIfEmpty(0)
            .Max() ?? 0;

        var now = DateTime.Now;
        _dbContext.ReconciliationOperationExecutions.Add(new ReconciliationOperationExecution
        {
            Id = Guid.NewGuid(),
            ReconciliationOperationId = operation.Id,
            ExecutionGroupId = operation.ExecutionGroupId,
            AttemptNo = Math.Max(maxPersistedAttemptNo, maxTrackedAttemptNo) + 1,
            StartedAt = operation.StartedAt ?? now,
            EndedAt = operation.CompletedAt ?? now,
            Outcome = outcome,
            ErrorCode = errorCode,
            ErrorText = errorText,
            RequestPayload = JsonSerializer.Serialize(new { operationCode = operation.OperationCode, source = "ReconciliationService" }),
            ResponsePayload = JsonSerializer.Serialize("Operation state changed outside executor."),
            CreateDate = now,
            CreatedBy = actor,
            UpdateDate = now,
            LastModifiedBy = actor,
            RecordStatus = RecordStatus.Active
        });
    }
}

// Decision/support layer: rule resolution, payload shaping, evaluation helpers and internal reconciliation types.
public partial class ReconciliationService
{
    private IQueryable<RecentClearingAggregate> BuildRecentClearingByCorrelationKeyQuery(Guid? importedFileId, int lookbackDays)
    {
        var clearingQuery = _dbContext.ClearingRecords
            .AsNoTracking()
            .Where(x => !string.IsNullOrWhiteSpace(x.CorrelationKey));

        if (importedFileId.HasValue)
        {
            clearingQuery = from clearing in clearingQuery
                            join importedFileRow in _dbContext.ImportedFileRows.AsNoTracking()
                                on clearing.ImportedFileRowId equals importedFileRow.Id
                            where importedFileRow.ImportedFileId == importedFileId.Value
                            select clearing;
        }
        else
        {
            var windowStart = DateTime.Now.AddDays(-Math.Max(1, lookbackDays));
            clearingQuery = clearingQuery.Where(x => x.CreateDate >= windowStart);
        }

        return clearingQuery
            .GroupBy(x => x.CorrelationKey!)
            .Select(g => new RecentClearingAggregate
            {
                CorrelationKey = g.Key,
                LatestClearingCreateDate = g.Max(x => x.CreateDate)
            });
    }

    private static Dictionary<string, string> BuildOperationPayloadMetadata(
        CardTransactionRecord card,
        Guid? clearingRecordId,
        Guid runId,
        Guid executionGroupId,
        FlowOperation operationDefinition)
    {
        var metadata = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["cardTransactionRecordId"] = card.Id.ToString(),
            ["clearingRecordId"] = clearingRecordId?.ToString() ?? string.Empty,
            ["runId"] = runId.ToString(),
            ["executionGroupId"] = executionGroupId.ToString(),
            ["correlationKey"] = card.CorrelationKey ?? string.Empty,
            ["oceanTxnGuid"] = card.OceanTxnGuid ?? string.Empty,
            ["operationReasonText"] = operationDefinition.ReasonText ?? string.Empty
        };

        foreach (var (key, value) in operationDefinition.Metadata)
        {
            if (!string.IsNullOrWhiteSpace(key))
            {
                metadata[key] = value ?? string.Empty;
            }
        }

        return metadata;
    }

    private static decimal? ResolveAccountingAmountForCard(CardTransactionRecord card) => card.OriginalAmount ?? card.SettlementAmount ?? card.CardHolderBillingAmount;
    private static string ResolveAccountingCurrencyForCard(CardTransactionRecord card) => !string.IsNullOrWhiteSpace(card.OriginalCurrency) ? card.OriginalCurrency : (!string.IsNullOrWhiteSpace(card.SettlementCurrency) ? card.SettlementCurrency : card.CardHolderBillingCurrency);
    private static string NormalizeAmount(decimal? amount) => amount.HasValue ? amount.Value.ToString("0.00", CultureInfo.InvariantCulture) : string.Empty;

    private static (CardReconciliationState State, string Reason) ResolvePlannedCardState(
        IReadOnlyCollection<FlowOperation> operations,
        string noActionDecisionCode)
    {
        if (operations.Count == 0)
        {
            return noActionDecisionCode switch
            {
                ReconciliationDecisionCodes.WaitingReevaluation => (CardReconciliationState.AwaitingReevaluation, ReconciliationDecisionCodes.WaitingReevaluation),
                ReconciliationDecisionCodes.WaitingClearing => (CardReconciliationState.AwaitingReevaluation, ReconciliationDecisionCodes.WaitingClearing),
                ReconciliationDecisionCodes.NoActionRequired => (CardReconciliationState.ReconcileCompleted, ReconciliationDecisionCodes.NoActionRequired),
                ReconciliationDecisionCodes.DuplicateConflictingSignature => (CardReconciliationState.ReconcileFailed, ReconciliationDecisionCodes.DuplicateConflictingSignature),
                ReconciliationDecisionCodes.DuplicateSameSignature => (CardReconciliationState.ReconcileCompleted, ReconciliationDecisionCodes.DuplicateSameSignature),
                _ => (CardReconciliationState.ReadyForReconcile, ReconciliationStateReasons.NoOperationPlanned)
            };
        }

        if (operations.Any(x => x.Mode == ReconciliationOperationMode.Manual))
        {
            return (CardReconciliationState.ManualReviewRequired, ReconciliationStateReasons.ManualOperationPlanned);
        }

        return (CardReconciliationState.ReadyForReconcile, ReconciliationStateReasons.AutoOperationPlanned);
    }

    private static ReconciliationDecision ResolveDecision(
        IReadOnlyCollection<FlowOperation> plannedOperations,
        Guid? clearingRecordId,
        string clearingControlStat,
        DuplicateRule duplicateRule)
    {
        if (plannedOperations.Count > 0)
        {
            var decisionType = plannedOperations.Any(x => x.Mode == ReconciliationOperationMode.Manual)
                ? ReconciliationDecisionType.ManualReviewRequired
                : ReconciliationDecisionType.ActionPlanCreated;
            return new ReconciliationDecision(decisionType, ReconciliationDecisionCodes.ActionPlanCreated, "Reconciliation planned one or more operations.");
        }

        var (reasonCode, reasonText) = ResolveNoActionReason(clearingRecordId, clearingControlStat, duplicateRule);
        return new ReconciliationDecision(ReconciliationDecisionType.NoAction, reasonCode, reasonText);
    }

    private static (string ReasonCode, string ReasonText) ResolveNoActionReason(
        Guid? clearingRecordId,
        string clearingControlStat,
        DuplicateRule duplicateRule)
    {
        if (duplicateRule is { ShouldProcess: false, DuplicateType: CardDuplicateType.ConflictingSignature })
        {
            return (ReconciliationDecisionCodes.DuplicateConflictingSignature, "Skipped because duplicate records have conflicting signatures.");
        }

        if (duplicateRule is { ShouldProcess: false, DuplicateType: CardDuplicateType.SameSignature })
        {
            return (ReconciliationDecisionCodes.DuplicateSameSignature, "Skipped because duplicate record has same signature and was already processed.");
        }

        if (!clearingRecordId.HasValue)
        {
            return (ReconciliationDecisionCodes.WaitingClearing, "No matching clearing record found yet; card transaction will wait for clearing.");
        }

        if (IsClearingAwaitingFinalization(clearingControlStat))
        {
            var normalizedControlStat = CardFlowText.Normalize(clearingControlStat);
            return (ReconciliationDecisionCodes.WaitingReevaluation, $"Clearing record is not final yet (ControlStat={normalizedControlStat}); card transaction will be re-evaluated in next run.");
        }

        return (ReconciliationDecisionCodes.NoActionRequired, "Reconciliation evaluated and no operation was required.");
    }

    private static bool IsClearingAwaitingFinalization(string clearingControlStat)
    {
        var normalizedControlStat = CardFlowText.Normalize(clearingControlStat);
        if (string.IsNullOrWhiteSpace(normalizedControlStat))
        {
            return false;
        }

        return normalizedControlStat != ReconciliationFieldValues.ClearingControlStatNormal
               && normalizedControlStat != ReconciliationFieldValues.ClearingControlStatProblemToNormal;
    }

    private static Dictionary<Guid, DuplicateRule> BuildDuplicateRules(IReadOnlyCollection<CardTransactionRecord> cards) =>
        CardDuplicateRuleBuilder.Build(cards);

    private int ResolveEvaluationDegreeOfParallelism()
    {
        if (_settings.ReconciliationProcessing?.EnableParallelEvaluation != true)
        {
            return 1;
        }

        return Math.Max(1, _settings.ReconciliationProcessing?.EvaluationDegreeOfParallelism ?? 4);
    }

    private sealed class EvaluationResult
    {
        public Guid? ClearingRecordId { get; init; }
        public string ClearingControlStat { get; init; }
        public CardFlowResult Flow { get; init; } = new();
    }

    private sealed record SelectedClearingInfo(Guid Id, ClearingProvider Provider, string ControlStat);

    private readonly record struct ReconciliationDecision(
        ReconciliationDecisionType DecisionType,
        string DecisionCode,
        string DecisionReason);

    private sealed class RecentClearingAggregate
    {
        public string CorrelationKey { get; init; }
        public DateTime LatestClearingCreateDate { get; init; }
    }
}
