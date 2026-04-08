using System.ComponentModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using LinkPara.Scheduler.API.Commons.Entities;
using LinkPara.Scheduler.API.Commons.Interfaces;
using LinkPara.SystemUser;
using MassTransit;

namespace LinkPara.Scheduler.API.Jobs.Card.FileIngestionAndReconciliation;

public static class CardFileIngestionAndReconciliationEndpointNames
{
    public const string SerialQueue = "queue:Card.FileIngestionAndReconciliation.SerialQueue";
}

#region ENUMS

public enum JobRequestType
{
    [Description("Ingest file request.")]
    IngestFile = 1,

    [Description("Evaluate request.")]
    Evaluate = 2,

    [Description("Execute request.")]
    Execute = 3
}

public enum FileSourceType
{
    [Description("Files are listed and read from remote sources (FTP/SFTP, etc.).")]
    Remote = 1,

    [Description("Local filesystem. If FilePath is provided, read directly from that path; if empty, list and read files from default path/profile in configuration.")]
    Local = 2
}

public enum FileType
{
    [Description("Card transaction file.")]
    Card = 1,

    [Description("Clearing/reconciliation file.")]
    Clearing = 2
}

public enum FileContentType
{
    [Description("File content in BKM format.")]
    Bkm = 1,

    [Description("File content in Mastercard (MSC) format.")]
    Msc = 2,

    [Description("File content in VISA format.")]
    Visa = 3
}

public enum FileIngestionAndReconciliationTemplate
{
    IngestRemoteCardBkm,
    IngestRemoteCardMsc,
    IngestRemoteCardVisa,

    IngestRemoteClearingBkm,
    IngestRemoteClearingMsc,
    IngestRemoteClearingVisa,

    IngestLocalCardBkm,
    IngestLocalCardMsc,
    IngestLocalCardVisa,

    IngestLocalClearingBkm,
    IngestLocalClearingMsc,
    IngestLocalClearingVisa,

    EvaluateDefault,
    ExecuteDefault
}

#endregion

#region REQUEST MODELS

public class FileIngestionAndReconciliationJobRequest
{
    [JsonConverter(typeof(FlexibleEnumJsonConverter<JobRequestType>))]
    public JobRequestType RequestType { get; set; }
    public string InitiatedByUserId { get; init; }
    public FileIngestionRequest IngestionRequest { get; init; }
    public EvaluateRequest EvaluateRequest { get; init; }
    public ExecuteRequest ExecuteRequest { get; init; }
}

public class FileIngestionRequest
{
    public object FileSourceType { get; set; } = default!;
    public object FileType { get; set; } = default!;
    public object FileContentType { get; set; } = default!;
    public string FilePath { get; set; }
}

public class EvaluateRequest
{
    public Guid[] IngestionFileIds { get; set; } = Array.Empty<Guid>();
    public EvaluateOptions Options { get; set; }
}

public class EvaluateOptions
{
    public int ChunkSize { get; set; }
    public int ClaimTimeoutSeconds { get; set; }
    public int ClaimRetryCount { get; set; }
}

public class ExecuteRequest
{
    public Guid[] GroupIds { get; set; } = Array.Empty<Guid>();
    public Guid[] OperationIds { get; set; } = Array.Empty<Guid>();
    public ExecuteOptions Options { get; set; }
}

public class ExecuteOptions
{
    public int MaxEvaluations { get; set; }
    public int LeaseSeconds { get; set; }
}

public sealed class FlexibleEnumJsonConverter<TEnum> : JsonConverter<TEnum>
    where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var rawValue = reader.GetString();

            if (string.IsNullOrWhiteSpace(rawValue))
                throw new JsonException($"Empty value is not valid for enum {typeof(TEnum).Name}.");

            if (Enum.TryParse<TEnum>(rawValue, ignoreCase: true, out var enumValue))
                return enumValue;

            if (int.TryParse(rawValue, out var numericValue) &&
                Enum.IsDefined(typeof(TEnum), numericValue))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), numericValue);
            }

            throw new JsonException($"Value '{rawValue}' is not valid for enum {typeof(TEnum).Name}.");
        }

        if (reader.TokenType == JsonTokenType.Number)
        {
            if (reader.TryGetInt32(out var numericValue) &&
                Enum.IsDefined(typeof(TEnum), numericValue))
            {
                return (TEnum)Enum.ToObject(typeof(TEnum), numericValue);
            }

            throw new JsonException($"Numeric value is not valid for enum {typeof(TEnum).Name}.");
        }

        throw new JsonException($"Token type '{reader.TokenType}' is not valid for enum {typeof(TEnum).Name}.");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(Convert.ToInt32(value));
    }
}

#endregion

#region FACTORY

public static class FileIngestionAndReconciliationPayloadFactory
{
    private const string DefaultLocalPath = "/Users/base/Documents/Files";

    /// <summary>
    /// false => enum values numeric gönderilir
    /// true  => enum values string gönderilir
    /// default kapalıdır
    /// </summary>
    public static bool UseStringEnums { get; private set; } = false;

    public static void SetUseStringEnums(bool useStringEnums)
    {
        UseStringEnums = useStringEnums;
    }

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
            RequestType = JobRequestType.Evaluate,
            InitiatedByUserId = initiatedByUserId,
            EvaluateRequest = new EvaluateRequest
            {
                IngestionFileIds = Array.Empty<Guid>(),
                Options = new EvaluateOptions
                {
                    ChunkSize = 10000,
                    ClaimTimeoutSeconds = 300,
                    ClaimRetryCount = 3
                }
            }
        },

        [FileIngestionAndReconciliationTemplate.ExecuteDefault] = initiatedByUserId => new FileIngestionAndReconciliationJobRequest
        {
            RequestType = JobRequestType.Execute,
            InitiatedByUserId = initiatedByUserId,
            ExecuteRequest = new ExecuteRequest
            {
                GroupIds = Array.Empty<Guid>(),
                OperationIds = Array.Empty<Guid>(),
                Options = new ExecuteOptions
                {
                    MaxEvaluations = 5000,
                    LeaseSeconds = 30
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

    public static FileIngestionAndReconciliationJobRequest Create(
        FileIngestionAndReconciliationTemplate template,
        string initiatedByUserId,
        bool useStringEnums)
    {
        var current = UseStringEnums;

        try
        {
            SetUseStringEnums(useStringEnums);
            return Create(template, initiatedByUserId);
        }
        finally
        {
            SetUseStringEnums(current);
        }
    }

    private static FileIngestionAndReconciliationJobRequest CreateIngestRequest(
        string initiatedByUserId,
        FileSourceType fileSourceType,
        FileType fileType,
        FileContentType fileContentType,
        string filePath = null)
    {
        return new FileIngestionAndReconciliationJobRequest
        {
            RequestType = JobRequestType.IngestFile,
            InitiatedByUserId = initiatedByUserId,
            IngestionRequest = new FileIngestionRequest
            {
                FileSourceType = EnumValue(fileSourceType),
                FileType = EnumValue(fileType),
                FileContentType = EnumValue(fileContentType),
                FilePath = filePath
            }
        };
    }

    private static object EnumValue<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        return UseStringEnums
            ? value.ToString()
            : Convert.ToInt32(value);
    }
}

#endregion

#region BASE JOBS

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

    protected async Task SendAsync(
        FileIngestionAndReconciliationTemplate template,
        bool useStringEnums,
        CancellationToken cancellationToken)
    {
        var endpoint = await _bus.GetSendEndpoint(
            new Uri(CardFileIngestionAndReconciliationEndpointNames.SerialQueue));

        var request = FileIngestionAndReconciliationPayloadFactory.Create(
            template,
            _applicationUserService.ApplicationUserId.ToString(),
            useStringEnums);

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

    public async Task TriggerAsync(CronJob job, bool useStringEnums)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await SendAsync(ImportTemplate, useStringEnums, cts.Token);
        await SendAsync(FileIngestionAndReconciliationTemplate.EvaluateDefault, useStringEnums, cts.Token);
        await SendAsync(FileIngestionAndReconciliationTemplate.ExecuteDefault, useStringEnums, cts.Token);
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

    public async Task TriggerAsync(CronJob job, bool useStringEnums)
    {
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

        await SendAsync(StepTemplate, useStringEnums, cts.Token);
    }
}

#endregion

#region REMOTE CARD

public class RemoteCardBkmJob : FileIngestionAndReconciliationPipelineJobBase
{
    public RemoteCardBkmJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestRemoteCardBkm;
}

public class RemoteCardMscJob : FileIngestionAndReconciliationPipelineJobBase
{
    public RemoteCardMscJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestRemoteCardMsc;
}

public class RemoteCardVisaJob : FileIngestionAndReconciliationPipelineJobBase
{
    public RemoteCardVisaJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestRemoteCardVisa;
}

#endregion

#region REMOTE CLEARING

public class RemoteClearingBkmJob : FileIngestionAndReconciliationPipelineJobBase
{
    public RemoteClearingBkmJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestRemoteClearingBkm;
}

public class RemoteClearingMscJob : FileIngestionAndReconciliationPipelineJobBase
{
    public RemoteClearingMscJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestRemoteClearingMsc;
}

public class RemoteClearingVisaJob : FileIngestionAndReconciliationPipelineJobBase
{
    public RemoteClearingVisaJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestRemoteClearingVisa;
}

#endregion

#region LOCAL CARD

public class LocalCardBkmJob : FileIngestionAndReconciliationPipelineJobBase
{
    public LocalCardBkmJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestLocalCardBkm;
}

public class LocalCardMscJob : FileIngestionAndReconciliationPipelineJobBase
{
    public LocalCardMscJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestLocalCardMsc;
}

public class LocalCardVisaJob : FileIngestionAndReconciliationPipelineJobBase
{
    public LocalCardVisaJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestLocalCardVisa;
}

#endregion

#region LOCAL CLEARING

public class LocalClearingBkmJob : FileIngestionAndReconciliationPipelineJobBase
{
    public LocalClearingBkmJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestLocalClearingBkm;
}

public class LocalClearingMscJob : FileIngestionAndReconciliationPipelineJobBase
{
    public LocalClearingMscJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestLocalClearingMsc;
}

public class LocalClearingVisaJob : FileIngestionAndReconciliationPipelineJobBase
{
    public LocalClearingVisaJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate ImportTemplate =>
        FileIngestionAndReconciliationTemplate.IngestLocalClearingVisa;
}

#endregion

#region SYSTEM

public class EvaluateDefaultJob : FileIngestionAndReconciliationSingleStepJobBase
{
    public EvaluateDefaultJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate StepTemplate =>
        FileIngestionAndReconciliationTemplate.EvaluateDefault;
}

public class ExecuteDefaultJob : FileIngestionAndReconciliationSingleStepJobBase
{
    public ExecuteDefaultJob(IBus bus, IApplicationUserService applicationUserService) : base(bus, applicationUserService) { }

    protected override FileIngestionAndReconciliationTemplate StepTemplate =>
        FileIngestionAndReconciliationTemplate.ExecuteDefault;
}

#endregion
