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

namespace LinkPara.Card.Application.Features.Reporting.Queries.ReconAdvanced;

public class GetFileReconSummaryQuery : SearchQueryParams, IRequest<GetFileReconSummaryResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? ContentType { get; set; }
    public FileType? FileType { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetFileReconSummaryQueryHandler : IRequestHandler<GetFileReconSummaryQuery, GetFileReconSummaryResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetFileReconSummaryQueryHandler> _logger;

    public GetFileReconSummaryQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetFileReconSummaryQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetFileReconSummaryResponse> Handle(GetFileReconSummaryQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetFileReconSummaryAsync(r, r.DataScope, r.ContentType, r.FileType, r.DateFrom, r.DateTo, ct);
            return new GetFileReconSummaryResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.FileReconSummaryFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FILE_RECON_SUMMARY");
            return new GetFileReconSummaryResponse
            { Message = _localizer.Get("Handler.Reporting.FileReconSummaryFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetReconMatchRateTrendQuery : IRequest<GetReconMatchRateTrendResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? Network { get; set; }
    public ReconSide? Side { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetReconMatchRateTrendQueryHandler : IRequestHandler<GetReconMatchRateTrendQuery, GetReconMatchRateTrendResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconMatchRateTrendQueryHandler> _logger;

    public GetReconMatchRateTrendQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetReconMatchRateTrendQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetReconMatchRateTrendResponse> Handle(GetReconMatchRateTrendQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconMatchRateTrendAsync(r.DataScope, r.Network, r.Side, r.DateFrom, r.DateTo, ct);
            return new GetReconMatchRateTrendResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconMatchRateTrendFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_MATCH_RATE_TREND");
            return new GetReconMatchRateTrendResponse
            { Message = _localizer.Get("Handler.Reporting.ReconMatchRateTrendFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetReconGapAnalysisQuery : IRequest<GetReconGapAnalysisResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? Network { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetReconGapAnalysisQueryHandler : IRequestHandler<GetReconGapAnalysisQuery, GetReconGapAnalysisResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconGapAnalysisQueryHandler> _logger;

    public GetReconGapAnalysisQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetReconGapAnalysisQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetReconGapAnalysisResponse> Handle(GetReconGapAnalysisQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconGapAnalysisAsync(r.DataScope, r.Network, r.DateFrom, r.DateTo, ct);
            return new GetReconGapAnalysisResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.ReconGapAnalysisFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_RECON_GAP_ANALYSIS");
            return new GetReconGapAnalysisResponse
            { Message = _localizer.Get("Handler.Reporting.ReconGapAnalysisFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetUnmatchedTransactionAgingQuery : IRequest<GetUnmatchedTransactionAgingResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? Network { get; set; }
    public ReconSide? Side { get; set; }
}

public class GetUnmatchedTransactionAgingQueryHandler : IRequestHandler<GetUnmatchedTransactionAgingQuery, GetUnmatchedTransactionAgingResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetUnmatchedTransactionAgingQueryHandler> _logger;

    public GetUnmatchedTransactionAgingQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetUnmatchedTransactionAgingQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetUnmatchedTransactionAgingResponse> Handle(GetUnmatchedTransactionAgingQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetUnmatchedTransactionAgingAsync(r.DataScope, r.Network, r.Side, ct);
            return new GetUnmatchedTransactionAgingResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.UnmatchedTxnAgingFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_UNMATCHED_TXN_AGING");
            return new GetUnmatchedTransactionAgingResponse
            { Message = _localizer.Get("Handler.Reporting.UnmatchedTxnAgingFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

public class GetNetworkReconScorecardQuery : IRequest<GetNetworkReconScorecardResponse>
{
    public DataScope? DataScope { get; set; }
    public FileContentType? Network { get; set; }
}

public class GetNetworkReconScorecardQueryHandler : IRequestHandler<GetNetworkReconScorecardQuery, GetNetworkReconScorecardResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetNetworkReconScorecardQueryHandler> _logger;

    public GetNetworkReconScorecardQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory, ILogger<GetNetworkReconScorecardQueryHandler> logger)
    { _svc = svc; _errorMapper = errorMapper; _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger; }

    public async Task<GetNetworkReconScorecardResponse> Handle(GetNetworkReconScorecardQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetNetworkReconScorecardAsync(r.DataScope, r.Network, ct);
            return new GetNetworkReconScorecardResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.NetworkReconScorecardFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_NETWORK_RECON_SCORECARD");
            return new GetNetworkReconScorecardResponse
            { Message = _localizer.Get("Handler.Reporting.NetworkReconScorecardFailed"), ErrorCount = 1, Errors = new List<ReconciliationErrorDetail> { error } };
        }
    }
}

