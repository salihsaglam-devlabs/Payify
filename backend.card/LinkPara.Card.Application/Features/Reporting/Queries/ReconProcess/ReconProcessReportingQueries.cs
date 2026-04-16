using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Domain.Enums.Reporting;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reporting.Queries.ReconProcess;

public class GetReconDailyOverviewQuery : IRequest<GetReconDailyOverviewResponse>
{
    public DataScope? DataScope { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetReconDailyOverviewQueryHandler : IRequestHandler<GetReconDailyOverviewQuery, GetReconDailyOverviewResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconDailyOverviewQueryHandler> _logger;

    public GetReconDailyOverviewQueryHandler(
        IReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetReconDailyOverviewQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetReconDailyOverviewResponse> Handle(GetReconDailyOverviewQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconDailyOverviewAsync(r.DataScope, r.DateFrom, r.DateTo, ct);
            return new GetReconDailyOverviewResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconDailyOverviewFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_DAILY_OVERVIEW");
            return new GetReconDailyOverviewResponse
            {
                Message = _localizer.Get("Handler.Reporting.ReconDailyOverviewFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

public class GetReconOpenItemsQuery : SearchQueryParams, IRequest<GetReconOpenItemsResponse>
{
    public OperationStatus? OperationStatus { get; set; }
    public string Branch { get; set; }
    public bool? IsManual { get; set; }
}

public class GetReconOpenItemsQueryHandler : IRequestHandler<GetReconOpenItemsQuery, GetReconOpenItemsResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconOpenItemsQueryHandler> _logger;

    public GetReconOpenItemsQueryHandler(
        IReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetReconOpenItemsQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetReconOpenItemsResponse> Handle(GetReconOpenItemsQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconOpenItemsAsync(r, r.OperationStatus, r.Branch, r.IsManual, ct);
            return new GetReconOpenItemsResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconOpenItemsFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_OPEN_ITEMS");
            return new GetReconOpenItemsResponse
            {
                Message = _localizer.Get("Handler.Reporting.ReconOpenItemsFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

public class GetReconOpenItemAgingQuery : IRequest<GetReconOpenItemAgingResponse> { }

public class GetReconOpenItemAgingQueryHandler : IRequestHandler<GetReconOpenItemAgingQuery, GetReconOpenItemAgingResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconOpenItemAgingQueryHandler> _logger;

    public GetReconOpenItemAgingQueryHandler(
        IReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetReconOpenItemAgingQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetReconOpenItemAgingResponse> Handle(GetReconOpenItemAgingQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconOpenItemAgingAsync(ct);
            return new GetReconOpenItemAgingResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconOpenItemAgingFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_OPEN_ITEM_AGING");
            return new GetReconOpenItemAgingResponse
            {
                Message = _localizer.Get("Handler.Reporting.ReconOpenItemAgingFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

public class GetReconManualReviewQueueQuery : SearchQueryParams, IRequest<GetReconManualReviewQueueResponse>
{
    public UrgencyLevel? UrgencyLevel { get; set; }
    public string OperationBranch { get; set; }
}

public class GetReconManualReviewQueueQueryHandler : IRequestHandler<GetReconManualReviewQueueQuery, GetReconManualReviewQueueResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconManualReviewQueueQueryHandler> _logger;

    public GetReconManualReviewQueueQueryHandler(
        IReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetReconManualReviewQueueQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetReconManualReviewQueueResponse> Handle(GetReconManualReviewQueueQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconManualReviewQueueAsync(r, r.UrgencyLevel, r.OperationBranch, ct);
            return new GetReconManualReviewQueueResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconManualReviewQueueFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_MANUAL_REVIEW_QUEUE");
            return new GetReconManualReviewQueueResponse
            {
                Message = _localizer.Get("Handler.Reporting.ReconManualReviewQueueFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

public class GetReconAlertSummaryQuery : IRequest<GetReconAlertSummaryResponse>
{
    public DataScope? DataScope { get; set; }
    public string Severity { get; set; }
    public string AlertType { get; set; }
    public AlertStatus? AlertStatus { get; set; }
}

public class GetReconAlertSummaryQueryHandler : IRequestHandler<GetReconAlertSummaryQuery, GetReconAlertSummaryResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconAlertSummaryQueryHandler> _logger;

    public GetReconAlertSummaryQueryHandler(
        IReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetReconAlertSummaryQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetReconAlertSummaryResponse> Handle(GetReconAlertSummaryQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconAlertSummaryAsync(r.DataScope, r.Severity, r.AlertType, r.AlertStatus, ct);
            return new GetReconAlertSummaryResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconAlertSummaryFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_ALERT_SUMMARY");
            return new GetReconAlertSummaryResponse
            {
                Message = _localizer.Get("Handler.Reporting.ReconAlertSummaryFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}
