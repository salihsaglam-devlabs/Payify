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

public class GetManualReviewPressureQuery : SearchQueryParams, IRequest<GetManualReviewPressureResponse>
{
    public DataScope? DataScope { get; set; }
    public string SlaBucket { get; set; }
    public string DefaultOnExpiry { get; set; }
    public string Currency { get; set; }
    public string Urgency { get; set; }
}

public class GetManualReviewPressureQueryHandler
    : IRequestHandler<GetManualReviewPressureQuery, GetManualReviewPressureResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetManualReviewPressureQueryHandler> _logger;

    public GetManualReviewPressureQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetManualReviewPressureQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetManualReviewPressureResponse> Handle(GetManualReviewPressureQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetManualReviewPressureAsync(r, r.DataScope, r.SlaBucket,
                r.DefaultOnExpiry, r.Currency, r.Urgency, ct);
            return new GetManualReviewPressureResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Operational.ManualReviewPressureFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_OPERATIONAL_MANUAL_REVIEW_PRESSURE");
            return new GetManualReviewPressureResponse
            {
                Message = _localizer.Get("Handler.Reporting.Operational.ManualReviewPressureFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

