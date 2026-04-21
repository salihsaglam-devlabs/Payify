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

public class GetResponseCodeDeclineHealthQuery : SearchQueryParams, IRequest<GetResponseCodeDeclineHealthResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string ResponseCode { get; set; }
    public string HealthFlag { get; set; }
    public string Urgency { get; set; }
}

public class GetResponseCodeDeclineHealthQueryHandler
    : IRequestHandler<GetResponseCodeDeclineHealthQuery, GetResponseCodeDeclineHealthResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetResponseCodeDeclineHealthQueryHandler> _logger;

    public GetResponseCodeDeclineHealthQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetResponseCodeDeclineHealthQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetResponseCodeDeclineHealthResponse> Handle(GetResponseCodeDeclineHealthQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetResponseCodeDeclineHealthAsync(r, r.DataScope, r.Network, r.ResponseCode,
                r.HealthFlag, r.Urgency, ct);
            return new GetResponseCodeDeclineHealthResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.ResponseCodeDeclineHealthFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_RESPONSE_CODE_DECLINE_HEALTH");
            return new GetResponseCodeDeclineHealthResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.ResponseCodeDeclineHealthFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

