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

public class GetLoyaltyPointsEconomyQuery : SearchQueryParams, IRequest<GetLoyaltyPointsEconomyResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string LoyaltyIntensity { get; set; }
    public string Urgency { get; set; }
}

public class GetLoyaltyPointsEconomyQueryHandler
    : IRequestHandler<GetLoyaltyPointsEconomyQuery, GetLoyaltyPointsEconomyResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetLoyaltyPointsEconomyQueryHandler> _logger;

    public GetLoyaltyPointsEconomyQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetLoyaltyPointsEconomyQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetLoyaltyPointsEconomyResponse> Handle(GetLoyaltyPointsEconomyQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetLoyaltyPointsEconomyAsync(r, r.DataScope, r.Network,
                r.DateFrom, r.DateTo, r.LoyaltyIntensity, r.Urgency, ct);
            return new GetLoyaltyPointsEconomyResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.LoyaltyPointsEconomyFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_LOYALTY_POINTS_ECONOMY");
            return new GetLoyaltyPointsEconomyResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.LoyaltyPointsEconomyFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

