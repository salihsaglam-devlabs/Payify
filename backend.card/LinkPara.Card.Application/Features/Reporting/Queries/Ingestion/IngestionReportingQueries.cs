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

namespace LinkPara.Card.Application.Features.Reporting.Queries.Ingestion;

public class GetIngestionFileOverviewQuery : SearchQueryParams, IRequest<GetIngestionFileOverviewResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? ContentType { get; set; }
    public FileType? FileType { get; set; }
    public FileStatus? FileStatus { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetIngestionFileOverviewQueryHandler : IRequestHandler<GetIngestionFileOverviewQuery, GetIngestionFileOverviewResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetIngestionFileOverviewQueryHandler> _logger;

    public GetIngestionFileOverviewQueryHandler(
        IReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetIngestionFileOverviewQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetIngestionFileOverviewResponse> Handle(GetIngestionFileOverviewQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetIngestionFileOverviewAsync(r, r.DataScope, r.ContentType, r.FileType, r.FileStatus, r.DateFrom, r.DateTo, ct);
            return new GetIngestionFileOverviewResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.IngestionFileOverviewFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_INGESTION_FILE_OVERVIEW");
            return new GetIngestionFileOverviewResponse
            {
                Message = _localizer.Get("Handler.Reporting.IngestionFileOverviewFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

public class GetIngestionFileQualityQuery : SearchQueryParams, IRequest<GetIngestionFileQualityResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? ContentType { get; set; }
    public FileType? FileType { get; set; }
    public FileStatus? FileStatus { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetIngestionFileQualityQueryHandler : IRequestHandler<GetIngestionFileQualityQuery, GetIngestionFileQualityResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetIngestionFileQualityQueryHandler> _logger;

    public GetIngestionFileQualityQueryHandler(
        IReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetIngestionFileQualityQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetIngestionFileQualityResponse> Handle(GetIngestionFileQualityQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetIngestionFileQualityAsync(r, r.DataScope, r.ContentType, r.FileType, r.FileStatus, r.DateFrom, r.DateTo, ct);
            return new GetIngestionFileQualityResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.IngestionFileQualityFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_INGESTION_FILE_QUALITY");
            return new GetIngestionFileQualityResponse
            {
                Message = _localizer.Get("Handler.Reporting.IngestionFileQualityFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

public class GetIngestionDailySummaryQuery : IRequest<GetIngestionDailySummaryResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? ContentType { get; set; }
    public FileType? FileType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetIngestionDailySummaryQueryHandler : IRequestHandler<GetIngestionDailySummaryQuery, GetIngestionDailySummaryResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetIngestionDailySummaryQueryHandler> _logger;

    public GetIngestionDailySummaryQueryHandler(
        IReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetIngestionDailySummaryQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetIngestionDailySummaryResponse> Handle(GetIngestionDailySummaryQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetIngestionDailySummaryAsync(r.DataScope, r.ContentType, r.FileType, r.DateFrom, r.DateTo, ct);
            return new GetIngestionDailySummaryResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.IngestionDailySummaryFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_INGESTION_DAILY_SUMMARY");
            return new GetIngestionDailySummaryResponse
            {
                Message = _localizer.Get("Handler.Reporting.IngestionDailySummaryFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

public class GetIngestionNetworkMatrixQuery : IRequest<GetIngestionNetworkMatrixResponse>
{
    public DataScope? DataScope { get; set; }
}

public class GetIngestionNetworkMatrixQueryHandler : IRequestHandler<GetIngestionNetworkMatrixQuery, GetIngestionNetworkMatrixResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetIngestionNetworkMatrixQueryHandler> _logger;

    public GetIngestionNetworkMatrixQueryHandler(
        IReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetIngestionNetworkMatrixQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetIngestionNetworkMatrixResponse> Handle(GetIngestionNetworkMatrixQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetIngestionNetworkMatrixAsync(r.DataScope, ct);
            return new GetIngestionNetworkMatrixResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.IngestionNetworkMatrixFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_INGESTION_NETWORK_MATRIX");
            return new GetIngestionNetworkMatrixResponse
            {
                Message = _localizer.Get("Handler.Reporting.IngestionNetworkMatrixFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

public class GetIngestionExceptionHotspotsQuery : SearchQueryParams, IRequest<GetIngestionExceptionHotspotsResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? ContentType { get; set; }
    public FileType? FileType { get; set; }
    public SeverityLevel? SeverityLevel { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetIngestionExceptionHotspotsQueryHandler : IRequestHandler<GetIngestionExceptionHotspotsQuery, GetIngestionExceptionHotspotsResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetIngestionExceptionHotspotsQueryHandler> _logger;

    public GetIngestionExceptionHotspotsQueryHandler(
        IReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetIngestionExceptionHotspotsQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetIngestionExceptionHotspotsResponse> Handle(GetIngestionExceptionHotspotsQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetIngestionExceptionHotspotsAsync(r, r.DataScope, r.ContentType, r.FileType, r.SeverityLevel, r.DateFrom, r.DateTo, ct);
            return new GetIngestionExceptionHotspotsResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.IngestionExceptionHotspotsFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_INGESTION_EXCEPTION_HOTSPOTS");
            return new GetIngestionExceptionHotspotsResponse
            {
                Message = _localizer.Get("Handler.Reporting.IngestionExceptionHotspotsFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}
