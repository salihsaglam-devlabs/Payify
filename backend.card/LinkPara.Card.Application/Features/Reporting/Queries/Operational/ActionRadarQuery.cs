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

public class GetActionRadarQuery : SearchQueryParams, IRequest<GetActionRadarResponse>
{
    public DataScope? DataScope { get; set; }
    public string Category { get; set; }
    public string IssueType { get; set; }
    public string Urgency { get; set; }
}

public class GetActionRadarQueryHandler
    : IRequestHandler<GetActionRadarQuery, GetActionRadarResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetActionRadarQueryHandler> _logger;

    public GetActionRadarQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetActionRadarQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetActionRadarResponse> Handle(GetActionRadarQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetActionRadarAsync(r, r.DataScope, r.Category, r.IssueType, r.Urgency, ct);
            return new GetActionRadarResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Operational.ActionRadarFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_OPERATIONAL_ACTION_RADAR");
            return new GetActionRadarResponse
            {
                Message = _localizer.Get("Handler.Reporting.Operational.ActionRadarFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

