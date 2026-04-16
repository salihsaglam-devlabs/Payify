using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reporting;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reporting.Queries.Archive;

public class GetArchiveRunOverviewQuery : SearchQueryParams, IRequest<GetArchiveRunOverviewResponse>
{
    public string ArchiveStatus { get; set; }
    public FileContentType? ContentType { get; set; }
    public FileType? FileType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetArchiveRunOverviewQueryHandler : IRequestHandler<GetArchiveRunOverviewQuery, GetArchiveRunOverviewResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetArchiveRunOverviewQueryHandler> _logger;

    public GetArchiveRunOverviewQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetArchiveRunOverviewQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetArchiveRunOverviewResponse> Handle(GetArchiveRunOverviewQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetArchiveRunOverviewAsync(r, r.ArchiveStatus, r.ContentType, r.FileType, r.DateFrom, r.DateTo, ct);
            return new GetArchiveRunOverviewResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ArchiveRunOverviewFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_ARCHIVE_RUN_OVERVIEW");
            return new GetArchiveRunOverviewResponse
            { Message = _localizer.Get("Handler.Reporting.ArchiveRunOverviewFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetArchiveEligibilityQuery : SearchQueryParams, IRequest<GetArchiveEligibilityResponse>
{
    public FileContentType? ContentType { get; set; }
    public FileType? FileType { get; set; }
    public ArchiveEligibilityStatus? ArchiveEligibilityStatus { get; set; }
}

public class GetArchiveEligibilityQueryHandler : IRequestHandler<GetArchiveEligibilityQuery, GetArchiveEligibilityResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetArchiveEligibilityQueryHandler> _logger;

    public GetArchiveEligibilityQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetArchiveEligibilityQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetArchiveEligibilityResponse> Handle(GetArchiveEligibilityQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetArchiveEligibilityAsync(r, r.ContentType, r.FileType, r.ArchiveEligibilityStatus, ct);
            return new GetArchiveEligibilityResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ArchiveEligibilityFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_ARCHIVE_ELIGIBILITY");
            return new GetArchiveEligibilityResponse
            { Message = _localizer.Get("Handler.Reporting.ArchiveEligibilityFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetArchiveBacklogTrendQuery : IRequest<GetArchiveBacklogTrendResponse>
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetArchiveBacklogTrendQueryHandler : IRequestHandler<GetArchiveBacklogTrendQuery, GetArchiveBacklogTrendResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetArchiveBacklogTrendQueryHandler> _logger;

    public GetArchiveBacklogTrendQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetArchiveBacklogTrendQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetArchiveBacklogTrendResponse> Handle(GetArchiveBacklogTrendQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetArchiveBacklogTrendAsync(r.DateFrom, r.DateTo, ct);
            return new GetArchiveBacklogTrendResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ArchiveBacklogTrendFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_ARCHIVE_BACKLOG_TREND");
            return new GetArchiveBacklogTrendResponse
            { Message = _localizer.Get("Handler.Reporting.ArchiveBacklogTrendFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetArchiveRetentionSnapshotQuery : IRequest<GetArchiveRetentionSnapshotResponse> { }

public class GetArchiveRetentionSnapshotQueryHandler : IRequestHandler<GetArchiveRetentionSnapshotQuery, GetArchiveRetentionSnapshotResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetArchiveRetentionSnapshotQueryHandler> _logger;

    public GetArchiveRetentionSnapshotQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetArchiveRetentionSnapshotQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetArchiveRetentionSnapshotResponse> Handle(GetArchiveRetentionSnapshotQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetArchiveRetentionSnapshotAsync(ct);
            return new GetArchiveRetentionSnapshotResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ArchiveRetentionSnapshotFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_ARCHIVE_RETENTION_SNAPSHOT");
            return new GetArchiveRetentionSnapshotResponse
            { Message = _localizer.Get("Handler.Reporting.ArchiveRetentionSnapshotFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}
