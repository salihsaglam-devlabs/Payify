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

public class GetMccRevenueConcentrationQuery : SearchQueryParams, IRequest<GetMccRevenueConcentrationResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string Mcc { get; set; }
    public string ConcentrationRisk { get; set; }
    public string Urgency { get; set; }
}

public class GetMccRevenueConcentrationQueryHandler
    : IRequestHandler<GetMccRevenueConcentrationQuery, GetMccRevenueConcentrationResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetMccRevenueConcentrationQueryHandler> _logger;

    public GetMccRevenueConcentrationQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetMccRevenueConcentrationQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetMccRevenueConcentrationResponse> Handle(GetMccRevenueConcentrationQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetMccRevenueConcentrationAsync(r, r.DataScope, r.Network, r.Mcc,
                r.ConcentrationRisk, r.Urgency, ct);
            return new GetMccRevenueConcentrationResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.MccRevenueConcentrationFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_MCC_REVENUE_CONCENTRATION");
            return new GetMccRevenueConcentrationResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.MccRevenueConcentrationFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

