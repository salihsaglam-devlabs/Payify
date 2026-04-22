using System.Text.Json;
using LinkPara.Card.Application.Commons.Exceptions;
using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces;
using LinkPara.Card.Application.Commons.Interfaces.Archive;
using LinkPara.Card.Application.Commons.Models.AppConfiguration;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Requests;
using LinkPara.Card.Application.Commons.Models.Archive.Contracts.Responses;
using LinkPara.Card.Infrastructure.Services.Audit;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LinkPara.Card.Infrastructure.Services.Archive;

internal sealed class ArchiveService : IArchiveService
{
    private static readonly HashSet<string> SupportedBeforeDateStrategies =
        new(StringComparer.OrdinalIgnoreCase) { "None", "RetentionDays" };

    private readonly ArchiveAggregateReader _reader;
    private readonly ArchiveEligibilityEvaluator _evaluator;
    private readonly CardConfigOptions.ArchiveEndpoints _options;
    private readonly IArchiveErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAuditStampService _auditStampService;
    private readonly ITimeProvider _timeProvider;

    public ArchiveService(
        ArchiveAggregateReader reader,
        ArchiveEligibilityEvaluator evaluator,
        IOptions<CardConfigOptions> options,
        IArchiveErrorMapper errorMapper,
        Func<LinkPara.Card.Application.Commons.Localization.LocalizerResource, IStringLocalizer> localizerFactory,
        IServiceProvider serviceProvider,
        IAuditStampService auditStampService,
        ITimeProvider timeProvider)
    {
        _reader = reader;
        _evaluator = evaluator;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LinkPara.Card.Application.Commons.Localization.LocalizerResource.Messages);
        _options = options.Value?.Endpoints?.Archive
            ?? throw new ArchiveOptionsNotConfiguredException(_localizer.Get("Config.Archive.OptionsNotConfigured"));
        _serviceProvider = serviceProvider;
        _auditStampService = auditStampService;
        _timeProvider = timeProvider;
    }

    public async Task<ArchivePreviewResponse> PreviewAsync(
        ArchivePreviewRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ArchiveErrorDetail>();
        try
        {
            var effectiveRequest = request ?? new ArchivePreviewRequest();
            var effectiveBeforeDate = ResolveBeforeDate(effectiveRequest.BeforeDate);
            var effectiveLimit = effectiveRequest.Limit is null or 0
                ? _options.Preview.PreviewLimit.Value
                : Math.Clamp(effectiveRequest.Limit.Value, 1, 10000);
            var candidateIds = await _reader.ResolveCandidateFileIdsAsync(
                effectiveRequest.IngestionFileIds ?? Array.Empty<Guid>(),
                effectiveBeforeDate,
                effectiveLimit,
                cancellationToken);

            var now = _timeProvider.Now;
            var response = new ArchivePreviewResponse();
            foreach (var candidateId in candidateIds)
            {
                try
                {
                    var snapshot = await _reader.GetSnapshotAsync(candidateId, cancellationToken);
                    var eligibility = _evaluator.Evaluate(snapshot, now);
                    response.Candidates.Add(new ArchiveCandidateResult
                    {
                        IngestionFileId = candidateId,
                        IsEligible = eligibility.IsEligible,
                        FailureReasons = eligibility.FailureReasons,
                        Counts = snapshot?.Counts ?? new ArchiveAggregateCounts()
                    });
                }
                catch (Exception ex)
                {
                    errors.Add(_errorMapper.MapException(ex, "ARCHIVE_PREVIEW_CANDIDATE", ingestionFileId: candidateId));
                    response.Candidates.Add(new ArchiveCandidateResult
                    {
                        IngestionFileId = candidateId,
                        IsEligible = false,
                        FailureReasons = new List<string> { "PREVIEW_EVALUATION_FAILED" },
                        Counts = new ArchiveAggregateCounts()
                    });
                }
            }

            response.Errors = errors;
            response.ErrorCount = errors.Count;
            return response;
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(ex, "ARCHIVE_SERVICE_PREVIEW"));
            return new ArchivePreviewResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Archive.PreviewFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    public async Task<ArchiveRunResponse> RunAsync(
        ArchiveRunRequest request,
        CancellationToken cancellationToken = default)
    {
        _auditStampService.EnsureAuditContext();
        var executor = _serviceProvider.GetRequiredService<ArchiveExecutor>();
        var errors = new List<ArchiveErrorDetail>();
        try
        {
            var effectiveRequest = request ?? new ArchiveRunRequest();
            var effectiveBeforeDate = ResolveBeforeDate(effectiveRequest.BeforeDate);
            var effectiveMaxFiles = effectiveRequest.MaxFiles is null or 0
                ? _options.Run.Defaults.MaxRunCount.Value
                : Math.Clamp(effectiveRequest.MaxFiles.Value, 1, 10000);
            var continueOnError = effectiveRequest.ContinueOnError ?? _options.Run.Defaults.ContinueOnError.Value;
            var candidateIds = await _reader.ResolveCandidateFileIdsAsync(
                effectiveRequest.IngestionFileIds ?? Array.Empty<Guid>(),
                effectiveBeforeDate,
                effectiveMaxFiles,
                cancellationToken);

            var filterJson = JsonSerializer.Serialize(effectiveRequest);
            var response = new ArchiveRunResponse();

            foreach (var candidateId in candidateIds)
            {
                ArchiveRunItemResult item = null!;
                var maxAttempts = _options.Run.Defaults.MaxRetryPerFile!.Value + 1;

                for (var attempt = 1; attempt <= maxAttempts; attempt++)
                {
                    try
                    {
                        item = await executor.ExecuteAsync(candidateId, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        var mappedError = _errorMapper.MapException(ex, "ARCHIVE_EXECUTE", ingestionFileId: candidateId);
                        errors.Add(mappedError);
                        item = new ArchiveRunItemResult
                        {
                            IngestionFileId = candidateId,
                            Status = "Failed",
                            Message = mappedError.Message,
                            FailureReasons = new List<string> { "ARCHIVE_EXECUTION_FAILED" }
                        };
                    }

                    if (item.Status != "Failed" || attempt >= maxAttempts)
                        break;

                    await Task.Delay(TimeSpan.FromSeconds(_options.Run.Defaults.RetryDelaySeconds!.Value), cancellationToken);
                }

                response.Items.Add(item);
                response.ProcessedCount++;

                switch (item.Status)
                {
                    case "Archived":
                        response.ArchivedCount++;
                        break;
                    case "Skipped":
                        response.SkippedCount++;
                        break;
                    default:
                        response.FailedCount++;
                        break;
                }

                try
                {
                    await executor.InsertArchiveLogAsync(item, filterJson, cancellationToken);
                }
                catch (Exception ex)
                {
                    errors.Add(_errorMapper.MapException(ex, "ARCHIVE_LOG_INSERT", ingestionFileId: candidateId));
                }

                if (item.Status == "Failed" && !continueOnError)
                {
                    break;
                }
            }


            response.Errors = errors;
            response.ErrorCount = errors.Count;
            return response;
        }
        catch (Exception ex)
        {
            errors.Add(_errorMapper.MapException(ex, "ARCHIVE_SERVICE_RUN"));
            return new ArchiveRunResponse
            {
                Message = BuildFailureMessage(_localizer.Get("Archive.RunFailed"), errors),
                Errors = errors,
                ErrorCount = errors.Count
            };
        }
    }

    private DateTime? ResolveBeforeDate(DateTime? requestBeforeDate)
    {
        if (_options.Run.Defaults.UseConfiguredBeforeDateOnly.Value)
        {
            return ResolveConfiguredBeforeDate();
        }

        return requestBeforeDate ?? ResolveConfiguredBeforeDate();
    }

    private DateTime? ResolveConfiguredBeforeDate()
    {
        var strategy = _options.Run.Defaults.DefaultBeforeDateStrategy;

        if (!SupportedBeforeDateStrategies.Contains(strategy))
        {
            throw new ArchiveUnsupportedBeforeDateStrategyException(
                _localizer.Get("Archive.UnsupportedBeforeDateStrategy", strategy, string.Join(", ", SupportedBeforeDateStrategies)));
        }

        return strategy switch
        {
            "RetentionDays" => _timeProvider.Now.AddDays(-_options.Run.Rules.RetentionDays.Value),
            "None" => null,
            _ => throw new ArchiveUnsupportedBeforeDateStrategyException(
                _localizer.Get("Archive.UnsupportedBeforeDateStrategy", strategy, string.Join(", ", SupportedBeforeDateStrategies)))
        };
    }


    private static string BuildFailureMessage(string fallbackMessage, IReadOnlyCollection<ArchiveErrorDetail> errors)
    {
        var firstMessage = errors.FirstOrDefault()?.Message;
        if (string.IsNullOrWhiteSpace(firstMessage))
        {
            return fallbackMessage;
        }

        return $"{fallbackMessage} {firstMessage}";
    }

}
