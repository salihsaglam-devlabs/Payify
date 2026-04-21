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

public class GetArchivePipelineHealthQuery : SearchQueryParams, IRequest<GetArchivePipelineHealthResponse>
{
    public DataScope? DataScope { get; set; }
    public string Perspective { get; set; }
    public string Side { get; set; }
    public string Network { get; set; }
    public string ArchiveStatus { get; set; }
    public string PipelineHealth { get; set; }
    public string Urgency { get; set; }
}

public class GetArchivePipelineHealthQueryHandler
    : IRequestHandler<GetArchivePipelineHealthQuery, GetArchivePipelineHealthResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetArchivePipelineHealthQueryHandler> _logger;

    public GetArchivePipelineHealthQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetArchivePipelineHealthQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetArchivePipelineHealthResponse> Handle(
        GetArchivePipelineHealthQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetArchivePipelineHealthAsync(r, r.DataScope, r.Perspective, r.Side,
                r.Network, r.ArchiveStatus, r.PipelineHealth, r.Urgency, ct);
            return new GetArchivePipelineHealthResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Operational.ArchivePipelineHealthFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_OPERATIONAL_ARCHIVE_PIPELINE_HEALTH");
            return new GetArchivePipelineHealthResponse
            {
                Message = _localizer.Get("Handler.Reporting.Operational.ArchivePipelineHealthFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

