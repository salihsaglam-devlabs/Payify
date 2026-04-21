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

public class GetCardClearingImbalanceQuery : SearchQueryParams, IRequest<GetCardClearingImbalanceResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string Currency { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string ImbalanceSeverity { get; set; }
    public string Urgency { get; set; }
}

public class GetCardClearingImbalanceQueryHandler
    : IRequestHandler<GetCardClearingImbalanceQuery, GetCardClearingImbalanceResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetCardClearingImbalanceQueryHandler> _logger;

    public GetCardClearingImbalanceQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetCardClearingImbalanceQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetCardClearingImbalanceResponse> Handle(
        GetCardClearingImbalanceQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetCardClearingImbalanceAsync(r, r.DataScope, r.Network, r.Currency,
                r.DateFrom, r.DateTo, r.ImbalanceSeverity, r.Urgency, ct);
            return new GetCardClearingImbalanceResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Operational.CardClearingImbalanceFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_OPERATIONAL_CARD_CLEARING_IMBALANCE");
            return new GetCardClearingImbalanceResponse
            {
                Message = _localizer.Get("Handler.Reporting.Operational.CardClearingImbalanceFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

