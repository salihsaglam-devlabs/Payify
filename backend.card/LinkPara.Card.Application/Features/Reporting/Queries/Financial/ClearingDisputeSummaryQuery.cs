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

namespace LinkPara.Card.Application.Features.Reporting.Queries.Financial;

public class GetClearingDisputeSummaryQuery : SearchQueryParams, IRequest<GetClearingDisputeSummaryResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string DisputeCode { get; set; }
    public string ReasonCode { get; set; }
    public string ControlStat { get; set; }
    public string DisputeFlag { get; set; }
    public string Urgency { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetClearingDisputeSummaryQueryHandler
    : IRequestHandler<GetClearingDisputeSummaryQuery, GetClearingDisputeSummaryResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetClearingDisputeSummaryQueryHandler> _logger;

    public GetClearingDisputeSummaryQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetClearingDisputeSummaryQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetClearingDisputeSummaryResponse> Handle(GetClearingDisputeSummaryQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetClearingDisputeSummaryAsync(r, r.DataScope, r.Network, r.DisputeCode,
                r.ReasonCode, r.ControlStat, r.DisputeFlag, r.Urgency, r.DateFrom, r.DateTo, ct);
            return new GetClearingDisputeSummaryResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.ClearingDisputeSummaryFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_CLEARING_DISPUTE_SUMMARY");
            return new GetClearingDisputeSummaryResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.ClearingDisputeSummaryFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

