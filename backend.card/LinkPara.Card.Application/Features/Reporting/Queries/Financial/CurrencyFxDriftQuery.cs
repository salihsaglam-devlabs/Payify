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

public class GetCurrencyFxDriftQuery : SearchQueryParams, IRequest<GetCurrencyFxDriftResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string OriginalCurrency { get; set; }
    public string SettlementCurrency { get; set; }
    public string BillingCurrency { get; set; }
    public string FxDriftSeverity { get; set; }
    public string Urgency { get; set; }
}

public class GetCurrencyFxDriftQueryHandler
    : IRequestHandler<GetCurrencyFxDriftQuery, GetCurrencyFxDriftResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetCurrencyFxDriftQueryHandler> _logger;

    public GetCurrencyFxDriftQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetCurrencyFxDriftQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetCurrencyFxDriftResponse> Handle(GetCurrencyFxDriftQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetCurrencyFxDriftAsync(r, r.DataScope, r.Network, r.OriginalCurrency,
                r.SettlementCurrency, r.BillingCurrency, r.FxDriftSeverity, r.Urgency, ct);
            return new GetCurrencyFxDriftResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.CurrencyFxDriftFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_CURRENCY_FX_DRIFT");
            return new GetCurrencyFxDriftResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.CurrencyFxDriftFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

