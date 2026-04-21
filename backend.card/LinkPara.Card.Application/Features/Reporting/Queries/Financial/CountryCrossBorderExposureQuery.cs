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

public class GetCountryCrossBorderExposureQuery : SearchQueryParams, IRequest<GetCountryCrossBorderExposureResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string MerchantCountry { get; set; }
    public string FxPattern { get; set; }
    public string OriginalCurrency { get; set; }
    public string SettlementCurrency { get; set; }
    public string ExposureFlag { get; set; }
    public string Urgency { get; set; }
}

public class GetCountryCrossBorderExposureQueryHandler
    : IRequestHandler<GetCountryCrossBorderExposureQuery, GetCountryCrossBorderExposureResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetCountryCrossBorderExposureQueryHandler> _logger;

    public GetCountryCrossBorderExposureQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetCountryCrossBorderExposureQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetCountryCrossBorderExposureResponse> Handle(GetCountryCrossBorderExposureQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetCountryCrossBorderExposureAsync(r, r.DataScope, r.Network, r.MerchantCountry,
                r.FxPattern, r.OriginalCurrency, r.SettlementCurrency, r.ExposureFlag, r.Urgency, ct);
            return new GetCountryCrossBorderExposureResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.CountryCrossBorderExposureFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_COUNTRY_CROSS_BORDER_EXPOSURE");
            return new GetCountryCrossBorderExposureResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.CountryCrossBorderExposureFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

