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

public class GetMisleadingSuccessCasesQuery : SearchQueryParams, IRequest<GetMisleadingSuccessCasesResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string Side { get; set; }
    public string Currency { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string MisleadingPattern { get; set; }
    public string Urgency { get; set; }
}

public class GetMisleadingSuccessCasesQueryHandler
    : IRequestHandler<GetMisleadingSuccessCasesQuery, GetMisleadingSuccessCasesResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetMisleadingSuccessCasesQueryHandler> _logger;

    public GetMisleadingSuccessCasesQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetMisleadingSuccessCasesQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetMisleadingSuccessCasesResponse> Handle(
        GetMisleadingSuccessCasesQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetMisleadingSuccessCasesAsync(r, r.DataScope, r.Network, r.Side, r.Currency,
                r.DateFrom, r.DateTo, r.MisleadingPattern, r.Urgency, ct);
            return new GetMisleadingSuccessCasesResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Operational.MisleadingSuccessCasesFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_OPERATIONAL_MISLEADING_SUCCESS_CASES");
            return new GetMisleadingSuccessCasesResponse
            {
                Message = _localizer.Get("Handler.Reporting.Operational.MisleadingSuccessCasesFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

