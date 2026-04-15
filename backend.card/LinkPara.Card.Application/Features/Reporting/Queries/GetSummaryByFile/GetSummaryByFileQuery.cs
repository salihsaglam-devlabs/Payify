using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Contracts.Responses;
using LinkPara.Card.Domain.Enums.Reporting;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reporting.Queries.GetSummaryByFile;

public class GetSummaryByFileQuery : IRequest<GetSummaryByFileResponse>
{
    public int? DateFrom { get; set; }
    public int? DateTo { get; set; }
    public ReportingNetwork? Network { get; set; }
}

public class GetSummaryByFileQueryHandler : IRequestHandler<GetSummaryByFileQuery, GetSummaryByFileResponse>
{
    private readonly IReportingService _service;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetSummaryByFileQueryHandler> _logger;

    public GetSummaryByFileQueryHandler(
        IReportingService service,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetSummaryByFileQueryHandler> logger)
    {
        _service = service;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetSummaryByFileResponse> Handle(GetSummaryByFileQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _service.GetSummaryByFileAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.GetSummaryFileFailed"));
            var error = _errorMapper.MapException(ex, "REPORTING_GET_SUMMARY_FILE");
            return new GetSummaryByFileResponse
            {
                Message = _localizer.Get("Handler.Reporting.GetSummaryFileFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}
