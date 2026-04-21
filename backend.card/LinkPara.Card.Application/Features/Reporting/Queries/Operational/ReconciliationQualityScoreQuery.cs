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

public class GetReconciliationQualityScoreQuery : SearchQueryParams, IRequest<GetReconciliationQualityScoreResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string QualityGrade { get; set; }
    public string WeakestDimension { get; set; }
    public string Urgency { get; set; }
}

public class GetReconciliationQualityScoreQueryHandler
    : IRequestHandler<GetReconciliationQualityScoreQuery, GetReconciliationQualityScoreResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconciliationQualityScoreQueryHandler> _logger;

    public GetReconciliationQualityScoreQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetReconciliationQualityScoreQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetReconciliationQualityScoreResponse> Handle(
        GetReconciliationQualityScoreQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconciliationQualityScoreAsync(r, r.DataScope, r.Network,
                r.DateFrom, r.DateTo, r.QualityGrade, r.WeakestDimension, r.Urgency, ct);
            return new GetReconciliationQualityScoreResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Operational.ReconciliationQualityScoreFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_OPERATIONAL_RECONCILIATION_QUALITY_SCORE");
            return new GetReconciliationQualityScoreResponse
            {
                Message = _localizer.Get("Handler.Reporting.Operational.ReconciliationQualityScoreFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

