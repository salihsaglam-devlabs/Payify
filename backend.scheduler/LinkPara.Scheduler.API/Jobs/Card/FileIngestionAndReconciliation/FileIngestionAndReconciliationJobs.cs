using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Enums;
using LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation.Models;
using LinkPara.SystemUser;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation;

public static class CardFileIngestionAndReconciliationEndpointNames
{
    public const string SerialQueue = "queue:Card.FileIngestionAndReconciliation.SerialQueue";
}

public static class FileIngestionAndReconciliationPayloadFactory
{
    private const string DefaultLocalPath = "/Users/base/Documents/Files";

    private static readonly Dictionary<FileIngestionAndReconciliationTemplate, Func<string, FileIngestionAndReconciliationJobRequest>> Templates = new()
    {
        [FileIngestionAndReconciliationTemplate.IngestRemoteCardBkm] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Remote,
            FileType.Card,
            FileContentType.Bkm),

        [FileIngestionAndReconciliationTemplate.IngestRemoteCardMsc] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Remote,
            FileType.Card,
            FileContentType.Msc),

        [FileIngestionAndReconciliationTemplate.IngestRemoteCardVisa] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Remote,
            FileType.Card,
            FileContentType.Visa),

        [FileIngestionAndReconciliationTemplate.IngestRemoteClearingBkm] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Remote,
            FileType.Clearing,
            FileContentType.Bkm),

        [FileIngestionAndReconciliationTemplate.IngestRemoteClearingMsc] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Remote,
            FileType.Clearing,
            FileContentType.Msc),

        [FileIngestionAndReconciliationTemplate.IngestRemoteClearingVisa] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Remote,
            FileType.Clearing,
            FileContentType.Visa),

        [FileIngestionAndReconciliationTemplate.IngestLocalCardBkm] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Local,
            FileType.Card,
            FileContentType.Bkm,
            DefaultLocalPath),

        [FileIngestionAndReconciliationTemplate.IngestLocalCardMsc] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Local,
            FileType.Card,
            FileContentType.Msc,
            DefaultLocalPath),

        [FileIngestionAndReconciliationTemplate.IngestLocalCardVisa] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Local,
            FileType.Card,
            FileContentType.Visa,
            DefaultLocalPath),

        [FileIngestionAndReconciliationTemplate.IngestLocalClearingBkm] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Local,
            FileType.Clearing,
            FileContentType.Bkm,
            DefaultLocalPath),

        [FileIngestionAndReconciliationTemplate.IngestLocalClearingMsc] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Local,
            FileType.Clearing,
            FileContentType.Msc,
            DefaultLocalPath),

        [FileIngestionAndReconciliationTemplate.IngestLocalClearingVisa] = initiatedByUserId => CreateIngestRequest(
            initiatedByUserId,
            FileSourceType.Local,
            FileType.Clearing,
            FileContentType.Visa,
            DefaultLocalPath),

        [FileIngestionAndReconciliationTemplate.EvaluateDefault] = initiatedByUserId => new FileIngestionAndReconciliationJobRequest
        {
            Type = JobRequestType.Evaluate,
            InitiatedByUserId = initiatedByUserId,
            EvaluateRequest = new EvaluateRequest
            {
                IngestionFileIds = Array.Empty<Guid>(),
                Options = new EvaluateOptions
                {
                    ChunkSize = 50_000,
                    ClaimTimeoutSeconds = 1_800,
                    ClaimRetryCount = 5,
                    OperationMaxRetries = 5
                }
            }
        },

        [FileIngestionAndReconciliationTemplate.ExecuteDefault] = initiatedByUserId => new FileIngestionAndReconciliationJobRequest
        {
            Type = JobRequestType.Execute,
            InitiatedByUserId = initiatedByUserId,
            ExecuteRequest = new ExecuteRequest
            {
                GroupIds = Array.Empty<Guid>(),
                EvaluationIds = Array.Empty<Guid>(),
                OperationIds = Array.Empty<Guid>(),
                Options = new ExecuteOptions
                {
                    MaxEvaluations = 500_000,
                    LeaseSeconds = 900
                }
            }
        }
    };

    public static FileIngestionAndReconciliationJobRequest Create(
        FileIngestionAndReconciliationTemplate template,
        string initiatedByUserId)
    {
        if (!Templates.TryGetValue(template, out var factory))
            throw new ArgumentException($"Unknown template: {template}", nameof(template));

        return factory(initiatedByUserId);
    }

    private static FileIngestionAndReconciliationJobRequest CreateIngestRequest(
        string initiatedByUserId,
        FileSourceType fileSourceType,
        FileType fileType,
        FileContentType fileContentType,
        string? filePath = null)
    {
        return new FileIngestionAndReconciliationJobRequest
        {
            Type = JobRequestType.IngestFile,
            InitiatedByUserId = initiatedByUserId,
            IngestionRequest = new FileIngestionRequest
            {
                FileSourceType = fileSourceType,
                FileType = fileType,
                FileContentType = fileContentType,
                FilePath = filePath
            }
        };
    }
}

public abstract class FileIngestionAndReconciliationJobBase : IJobTrigger
{
    private readonly IBus _bus;
    private readonly IApplicationUserService _applicationUserService;

    protected FileIngestionAndReconciliationJobBase(
        IBus bus,
        IApplicationUserService applicationUserService)
    {
        _bus = bus;
        _applicationUserService = applicationUserService;
    }

    protected async Task SendAsync(
        FileIngestionAndReconciliationTemplate template,
        CancellationToken cancellationToken)
    {
        var endpoint = await _bus.GetSendEndpoint(
            new Uri(CardFileIngestionAndReconciliationEndpointNames.SerialQueue));

        var request = FileIngestionAndReconciliationPayloadFactory.Create(
            template,
            _applicationUserService.ApplicationUserId.ToString());

        await endpoint.Send(request, cancellationToken);
    }

    public abstract Task TriggerAsync(CronJob job);
}

public abstract class FileIngestionAndReconciliationPipelineJobBase : FileIngestionAndReconciliationJobBase
{
    protected FileIngestionAndReconciliationPipelineJobBase(
        IBus bus,
        IApplicationUserService applicationUserService)
        : base(bus, applicationUserService)
    {
    }

    protected abstract FileIngestionAndReconciliationTemplate ImportTemplate { get; }

    public override async Task TriggerAsync(CronJob job)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await SendAsync(ImportTemplate, cts.Token);
        await SendAsync(FileIngestionAndReconciliationTemplate.EvaluateDefault, cts.Token);
        await SendAsync(FileIngestionAndReconciliationTemplate.ExecuteDefault, cts.Token);
    }
}

public abstract class FileIngestionAndReconciliationSingleStepJobBase : FileIngestionAndReconciliationJobBase
{
    protected FileIngestionAndReconciliationSingleStepJobBase(
        IBus bus,
        IApplicationUserService applicationUserService)
        : base(bus, applicationUserService)
    {
    }

    protected abstract FileIngestionAndReconciliationTemplate StepTemplate { get; }

    public override async Task TriggerAsync(CronJob job)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await SendAsync(StepTemplate, cts.Token);
    }
}