using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reporting;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Application.Commons.Models.Reporting.Dynamic;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reporting.Queries.Dynamic;

public class GetDynamicReportingQuery : IRequest<DynamicReportingResponse>
{
    public string? ReportName { get; set; }
    
    public bool IncludeContract { get; set; } = true;
    
    public bool IncludeData { get; set; } = false;
    
    public List<DynamicReportingFilter> Filters { get; set; } = new();
}

public class GetDynamicReportingQueryHandler
    : IRequestHandler<GetDynamicReportingQuery, DynamicReportingResponse>
{
    private readonly IDynamicReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetDynamicReportingQueryHandler> _logger;

    public GetDynamicReportingQueryHandler(
        IDynamicReportingService svc,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetDynamicReportingQueryHandler> logger)
    {
        _svc = svc;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<DynamicReportingResponse> Handle(GetDynamicReportingQuery request, CancellationToken ct)
    {
        try
        {
            return await _svc.ExecuteAsync(request, ct);
        }
        catch (Exception ex)
        {
            var msg = _localizer.Get("Handler.Reporting.Dynamic.Failed");
            _logger.LogError(ex, msg);
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_DYNAMIC");
            return new DynamicReportingResponse
            {
                ReportName = request.ReportName,
                Message = msg,
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

