using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.Card.Domain.Enums.Reporting;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reporting.Queries.Operational;

public class GetUnmatchedFinancialExposureQuery : SearchQueryParams, IRequest<GetUnmatchedFinancialExposureResponse>
{
    public DataScope? DataScope { get; set; }
    public string Side { get; set; }
    public string Network { get; set; }
    public string Currency { get; set; }
    public string AgingBucket { get; set; }
    public string RiskFlag { get; set; }
    public string Urgency { get; set; }
}

public class GetUnmatchedFinancialExposureQueryHandler
    : IRequestHandler<GetUnmatchedFinancialExposureQuery, GetUnmatchedFinancialExposureResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetUnmatchedFinancialExposureQueryHandler> _logger;

    public GetUnmatchedFinancialExposureQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetUnmatchedFinancialExposureQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetUnmatchedFinancialExposureResponse> Handle(
        GetUnmatchedFinancialExposureQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetUnmatchedFinancialExposureAsync(r, r.DataScope, r.Side, r.Network,
                r.Currency, r.AgingBucket, r.RiskFlag, r.Urgency, ct);
            return new GetUnmatchedFinancialExposureResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Operational.UnmatchedFinancialExposureFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_OPERATIONAL_UNMATCHED_FINANCIAL_EXPOSURE");
            return new GetUnmatchedFinancialExposureResponse
            {
                Message = _localizer.Get("Handler.Reporting.Operational.UnmatchedFinancialExposureFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

