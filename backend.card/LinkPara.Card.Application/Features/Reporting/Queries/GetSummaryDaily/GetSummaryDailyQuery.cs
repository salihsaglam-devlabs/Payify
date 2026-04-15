using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryDaily;

public class GetSummaryDailyQuery : IRequest<GetSummaryDailyResponse>
{
    public int? DateFrom { get; set; }
    public int? DateTo { get; set; }
}

public class GetSummaryDailyQueryHandler : IRequestHandler<GetSummaryDailyQuery, GetSummaryDailyResponse>
{
    private readonly IReportingService _service;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetSummaryDailyQueryHandler> _logger;

    public GetSummaryDailyQueryHandler(
        IReportingService service,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetSummaryDailyQueryHandler> logger)
    {
        _service = service;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetSummaryDailyResponse> Handle(GetSummaryDailyQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _service.GetSummaryDailyAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.GetSummaryDailyFailed"));
            var error = _errorMapper.MapException(ex, "REPORTING_GET_SUMMARY_DAILY");
            return new GetSummaryDailyResponse
            {
                Message = _localizer.Get("Handler.Reporting.GetSummaryDailyFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}
