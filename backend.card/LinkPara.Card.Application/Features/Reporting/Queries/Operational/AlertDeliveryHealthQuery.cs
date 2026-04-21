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

public class GetAlertDeliveryHealthQuery : SearchQueryParams, IRequest<GetAlertDeliveryHealthResponse>
{
    public DataScope? DataScope { get; set; }
    public string Severity { get; set; }
    public string AlertType { get; set; }
    public string DeliveryHealthStatus { get; set; }
    public string Urgency { get; set; }
}

public class GetAlertDeliveryHealthQueryHandler
    : IRequestHandler<GetAlertDeliveryHealthQuery, GetAlertDeliveryHealthResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetAlertDeliveryHealthQueryHandler> _logger;

    public GetAlertDeliveryHealthQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetAlertDeliveryHealthQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetAlertDeliveryHealthResponse> Handle(GetAlertDeliveryHealthQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetAlertDeliveryHealthAsync(r, r.DataScope, r.Severity, r.AlertType,
                r.DeliveryHealthStatus, r.Urgency, ct);
            return new GetAlertDeliveryHealthResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Operational.AlertDeliveryHealthFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_OPERATIONAL_ALERT_DELIVERY_HEALTH");
            return new GetAlertDeliveryHealthResponse
            {
                Message = _localizer.Get("Handler.Reporting.Operational.AlertDeliveryHealthFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

