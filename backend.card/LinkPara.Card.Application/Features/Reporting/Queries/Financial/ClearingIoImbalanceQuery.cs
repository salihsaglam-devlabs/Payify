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

public class GetClearingIoImbalanceQuery : SearchQueryParams, IRequest<GetClearingIoImbalanceResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string ImbalanceFlag { get; set; }
    public string Urgency { get; set; }
}

public class GetClearingIoImbalanceQueryHandler
    : IRequestHandler<GetClearingIoImbalanceQuery, GetClearingIoImbalanceResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetClearingIoImbalanceQueryHandler> _logger;

    public GetClearingIoImbalanceQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetClearingIoImbalanceQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetClearingIoImbalanceResponse> Handle(GetClearingIoImbalanceQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetClearingIoImbalanceAsync(r, r.DataScope, r.Network,
                r.DateFrom, r.DateTo, r.ImbalanceFlag, r.Urgency, ct);
            return new GetClearingIoImbalanceResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.ClearingIoImbalanceFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_CLEARING_IO_IMBALANCE");
            return new GetClearingIoImbalanceResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.ClearingIoImbalanceFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

