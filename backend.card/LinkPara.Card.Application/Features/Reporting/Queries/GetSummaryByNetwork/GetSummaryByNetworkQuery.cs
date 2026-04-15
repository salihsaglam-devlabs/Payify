using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryByNetwork;

public class GetSummaryByNetworkQuery : IRequest<GetSummaryByNetworkResponse>
{
    public int? DateFrom { get; set; }
    public int? DateTo { get; set; }
}

public class GetSummaryByNetworkQueryHandler : IRequestHandler<GetSummaryByNetworkQuery, GetSummaryByNetworkResponse>
{
    private readonly IReportingService _service;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetSummaryByNetworkQueryHandler> _logger;

    public GetSummaryByNetworkQueryHandler(
        IReportingService service,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetSummaryByNetworkQueryHandler> logger)
    {
        _service = service;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetSummaryByNetworkResponse> Handle(GetSummaryByNetworkQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _service.GetSummaryByNetworkAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.GetSummaryNetworkFailed"));
            var error = _errorMapper.MapException(ex, "REPORTING_GET_SUMMARY_NETWORK");
            return new GetSummaryByNetworkResponse
            {
                Message = _localizer.Get("Handler.Reporting.GetSummaryNetworkFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}
