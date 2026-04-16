using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.Card.Domain.Enums.FileIngestion;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.Card.Domain.Enums.Reporting;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reporting.Queries.ReconContent;

public class GetReconLiveCardContentDailyQuery : SearchQueryParams, IRequest<GetReconCardContentDailyResponse>
{
    public FileContentType? Network { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetReconLiveCardContentDailyQueryHandler : IRequestHandler<GetReconLiveCardContentDailyQuery, GetReconCardContentDailyResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconLiveCardContentDailyQueryHandler> _logger;

    public GetReconLiveCardContentDailyQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetReconLiveCardContentDailyQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetReconCardContentDailyResponse> Handle(GetReconLiveCardContentDailyQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconLiveCardContentDailyAsync(r, r.Network, r.DateFrom, r.DateTo, ct);
            return new GetReconCardContentDailyResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconLiveCardContentDailyFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_LIVE_CARD_CONTENT_DAILY");
            return new GetReconCardContentDailyResponse
            { Message = _localizer.Get("Handler.Reporting.ReconLiveCardContentDailyFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetReconLiveClearingContentDailyQuery : SearchQueryParams, IRequest<GetReconClearingContentDailyResponse>
{
    public FileContentType? Network { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetReconLiveClearingContentDailyQueryHandler : IRequestHandler<GetReconLiveClearingContentDailyQuery, GetReconClearingContentDailyResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconLiveClearingContentDailyQueryHandler> _logger;

    public GetReconLiveClearingContentDailyQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetReconLiveClearingContentDailyQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetReconClearingContentDailyResponse> Handle(GetReconLiveClearingContentDailyQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconLiveClearingContentDailyAsync(r, r.Network, r.DateFrom, r.DateTo, ct);
            return new GetReconClearingContentDailyResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconLiveClearingContentDailyFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_LIVE_CLEARING_CONTENT_DAILY");
            return new GetReconClearingContentDailyResponse
            { Message = _localizer.Get("Handler.Reporting.ReconLiveClearingContentDailyFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetReconArchiveCardContentDailyQuery : SearchQueryParams, IRequest<GetReconCardContentDailyResponse>
{
    public FileContentType? Network { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetReconArchiveCardContentDailyQueryHandler : IRequestHandler<GetReconArchiveCardContentDailyQuery, GetReconCardContentDailyResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconArchiveCardContentDailyQueryHandler> _logger;

    public GetReconArchiveCardContentDailyQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetReconArchiveCardContentDailyQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetReconCardContentDailyResponse> Handle(GetReconArchiveCardContentDailyQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconArchiveCardContentDailyAsync(r, r.Network, r.DateFrom, r.DateTo, ct);
            return new GetReconCardContentDailyResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconArchiveCardContentDailyFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_ARCHIVE_CARD_CONTENT_DAILY");
            return new GetReconCardContentDailyResponse
            { Message = _localizer.Get("Handler.Reporting.ReconArchiveCardContentDailyFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetReconArchiveClearingContentDailyQuery : SearchQueryParams, IRequest<GetReconClearingContentDailyResponse>
{
    public FileContentType? Network { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetReconArchiveClearingContentDailyQueryHandler : IRequestHandler<GetReconArchiveClearingContentDailyQuery, GetReconClearingContentDailyResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconArchiveClearingContentDailyQueryHandler> _logger;

    public GetReconArchiveClearingContentDailyQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetReconArchiveClearingContentDailyQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetReconClearingContentDailyResponse> Handle(GetReconArchiveClearingContentDailyQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconArchiveClearingContentDailyAsync(r, r.Network, r.DateFrom, r.DateTo, ct);
            return new GetReconClearingContentDailyResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconArchiveClearingContentDailyFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_ARCHIVE_CLEARING_CONTENT_DAILY");
            return new GetReconClearingContentDailyResponse
            { Message = _localizer.Get("Handler.Reporting.ReconArchiveClearingContentDailyFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetReconContentDailyQuery : SearchQueryParams, IRequest<GetReconContentDailyResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? Network { get; set; }
    public ReconSide? Side { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetReconContentDailyQueryHandler : IRequestHandler<GetReconContentDailyQuery, GetReconContentDailyResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconContentDailyQueryHandler> _logger;

    public GetReconContentDailyQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetReconContentDailyQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetReconContentDailyResponse> Handle(GetReconContentDailyQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconContentDailyAsync(r, r.DataScope, r.Network, r.Side, r.DateFrom, r.DateTo, ct);
            return new GetReconContentDailyResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconContentDailyFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_CONTENT_DAILY");
            return new GetReconContentDailyResponse
            { Message = _localizer.Get("Handler.Reporting.ReconContentDailyFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetReconClearingControlStatAnalysisQuery : IRequest<GetReconClearingControlStatAnalysisResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? Network { get; set; }
}

public class GetReconClearingControlStatAnalysisQueryHandler : IRequestHandler<GetReconClearingControlStatAnalysisQuery, GetReconClearingControlStatAnalysisResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconClearingControlStatAnalysisQueryHandler> _logger;

    public GetReconClearingControlStatAnalysisQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetReconClearingControlStatAnalysisQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetReconClearingControlStatAnalysisResponse> Handle(GetReconClearingControlStatAnalysisQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconClearingControlStatAnalysisAsync(r.DataScope, r.Network, ct);
            return new GetReconClearingControlStatAnalysisResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconClearingControlStatFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_CLEARING_CONTROL_STAT");
            return new GetReconClearingControlStatAnalysisResponse
            { Message = _localizer.Get("Handler.Reporting.ReconClearingControlStatFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetReconFinancialSummaryQuery : IRequest<GetReconFinancialSummaryResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? Network { get; set; }
    public string FinancialType { get; set; }
    public string TxnEffect { get; set; }
    public int? OriginalCurrency { get; set; }
}

public class GetReconFinancialSummaryQueryHandler : IRequestHandler<GetReconFinancialSummaryQuery, GetReconFinancialSummaryResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconFinancialSummaryQueryHandler> _logger;

    public GetReconFinancialSummaryQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetReconFinancialSummaryQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetReconFinancialSummaryResponse> Handle(GetReconFinancialSummaryQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconFinancialSummaryAsync(r.DataScope, r.Network, r.FinancialType, r.TxnEffect, r.OriginalCurrency, ct);
            return new GetReconFinancialSummaryResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconFinancialSummaryFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_FINANCIAL_SUMMARY");
            return new GetReconFinancialSummaryResponse
            { Message = _localizer.Get("Handler.Reporting.ReconFinancialSummaryFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetReconResponseStatusAnalysisQuery : IRequest<GetReconResponseStatusAnalysisResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? Network { get; set; }
    public ReconciliationStatus? ReconciliationStatus { get; set; }
}

public class GetReconResponseStatusAnalysisQueryHandler : IRequestHandler<GetReconResponseStatusAnalysisQuery, GetReconResponseStatusAnalysisResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconResponseStatusAnalysisQueryHandler> _logger;

    public GetReconResponseStatusAnalysisQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetReconResponseStatusAnalysisQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetReconResponseStatusAnalysisResponse> Handle(GetReconResponseStatusAnalysisQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconResponseStatusAnalysisAsync(r.DataScope, r.Network, r.ReconciliationStatus, ct);
            return new GetReconResponseStatusAnalysisResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconResponseStatusFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_RESPONSE_STATUS");
            return new GetReconResponseStatusAnalysisResponse
            { Message = _localizer.Get("Handler.Reporting.ReconResponseStatusFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}
