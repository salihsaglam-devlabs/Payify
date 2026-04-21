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

public class GetMerchantRiskHotspotsQuery : SearchQueryParams, IRequest<GetMerchantRiskHotspotsResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string MerchantId { get; set; }
    public string MerchantCountry { get; set; }
    public string RiskFlag { get; set; }
    public string Urgency { get; set; }
}

public class GetMerchantRiskHotspotsQueryHandler
    : IRequestHandler<GetMerchantRiskHotspotsQuery, GetMerchantRiskHotspotsResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetMerchantRiskHotspotsQueryHandler> _logger;

    public GetMerchantRiskHotspotsQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetMerchantRiskHotspotsQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetMerchantRiskHotspotsResponse> Handle(GetMerchantRiskHotspotsQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetMerchantRiskHotspotsAsync(r, r.DataScope, r.Network, r.MerchantId,
                r.MerchantCountry, r.RiskFlag, r.Urgency, ct);
            return new GetMerchantRiskHotspotsResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.MerchantRiskHotspotsFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_MERCHANT_RISK_HOTSPOTS");
            return new GetMerchantRiskHotspotsResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.MerchantRiskHotspotsFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

