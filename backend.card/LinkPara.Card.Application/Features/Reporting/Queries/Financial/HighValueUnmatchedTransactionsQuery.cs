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

public class GetHighValueUnmatchedTransactionsQuery : SearchQueryParams, IRequest<GetHighValueUnmatchedTransactionsResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string MerchantCountry { get; set; }
    public string Currency { get; set; }
    public decimal? MinAmount { get; set; }
    public string RiskFlag { get; set; }
    public string Urgency { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
}

public class GetHighValueUnmatchedTransactionsQueryHandler
    : IRequestHandler<GetHighValueUnmatchedTransactionsQuery, GetHighValueUnmatchedTransactionsResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetHighValueUnmatchedTransactionsQueryHandler> _logger;

    public GetHighValueUnmatchedTransactionsQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetHighValueUnmatchedTransactionsQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetHighValueUnmatchedTransactionsResponse> Handle(GetHighValueUnmatchedTransactionsQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetHighValueUnmatchedTransactionsAsync(r, r.DataScope, r.Network, r.MerchantCountry,
                r.Currency, r.MinAmount, r.RiskFlag, r.Urgency, r.DateFrom, r.DateTo, ct);
            return new GetHighValueUnmatchedTransactionsResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.HighValueUnmatchedTransactionsFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_HIGH_VALUE_UNMATCHED_TRANSACTIONS");
            return new GetHighValueUnmatchedTransactionsResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.HighValueUnmatchedTransactionsFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

