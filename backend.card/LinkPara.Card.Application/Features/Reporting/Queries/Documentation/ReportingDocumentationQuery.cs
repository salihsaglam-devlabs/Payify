using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reporting.Queries.Documentation;

public class GetReportingDocumentationQuery : SearchQueryParams, IRequest<GetReportingDocumentationResponse>
{
    public string ViewName { get; set; }
    public string ReportGroup { get; set; }
}

public class GetReportingDocumentationQueryHandler
    : IRequestHandler<GetReportingDocumentationQuery, GetReportingDocumentationResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReportingDocumentationQueryHandler> _logger;

    public GetReportingDocumentationQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetReportingDocumentationQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetReportingDocumentationResponse> Handle(
        GetReportingDocumentationQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReportingDocumentationAsync(r, r.ViewName, r.ReportGroup, ct);
            return new GetReportingDocumentationResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Documentation.ReportingDocumentationFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_DOCUMENTATION_REPORTING_DOCUMENTATION");
            return new GetReportingDocumentationResponse
            {
                Message = _localizer.Get("Handler.Reporting.Documentation.ReportingDocumentationFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

