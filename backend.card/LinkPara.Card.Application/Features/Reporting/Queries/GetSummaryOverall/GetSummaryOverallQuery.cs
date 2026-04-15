using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryOverall;

public class GetSummaryOverallQuery : IRequest<GetSummaryOverallResponse> { }

public class GetSummaryOverallQueryHandler : IRequestHandler<GetSummaryOverallQuery, GetSummaryOverallResponse>
{
    private readonly IReportingService _service;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetSummaryOverallQueryHandler> _logger;

    public GetSummaryOverallQueryHandler(
        IReportingService service,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetSummaryOverallQueryHandler> logger)
    {
        _service = service;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetSummaryOverallResponse> Handle(GetSummaryOverallQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _service.GetSummaryOverallAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.GetSummaryOverallFailed"));
            var error = _errorMapper.MapException(ex, "REPORTING_GET_SUMMARY_OVERALL");
            return new GetSummaryOverallResponse
            {
                Message = _localizer.Get("Handler.Reporting.GetSummaryOverallFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}
