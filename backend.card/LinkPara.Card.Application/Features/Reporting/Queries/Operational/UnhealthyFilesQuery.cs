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

public class GetUnhealthyFilesQuery : SearchQueryParams, IRequest<GetUnhealthyFilesResponse>
{
    public DataScope? DataScope { get; set; }
    public string Side { get; set; }
    public string Network { get; set; }
    public string FileStatus { get; set; }
    public string IssueCategory { get; set; }
    public string Urgency { get; set; }
}

public class GetUnhealthyFilesQueryHandler
    : IRequestHandler<GetUnhealthyFilesQuery, GetUnhealthyFilesResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetUnhealthyFilesQueryHandler> _logger;

    public GetUnhealthyFilesQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetUnhealthyFilesQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetUnhealthyFilesResponse> Handle(GetUnhealthyFilesQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetUnhealthyFilesAsync(r, r.DataScope, r.Side, r.Network,
                r.FileStatus, r.IssueCategory, r.Urgency, ct);
            return new GetUnhealthyFilesResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Operational.UnhealthyFilesFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_OPERATIONAL_UNHEALTHY_FILES");
            return new GetUnhealthyFilesResponse
            {
                Message = _localizer.Get("Handler.Reporting.Operational.UnhealthyFilesFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

