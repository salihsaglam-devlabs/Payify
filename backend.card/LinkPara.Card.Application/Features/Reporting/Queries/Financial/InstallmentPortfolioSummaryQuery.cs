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

public class GetInstallmentPortfolioSummaryQuery : SearchQueryParams, IRequest<GetInstallmentPortfolioSummaryResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string InstallmentBucket { get; set; }
    public string PortfolioFlag { get; set; }
    public string Urgency { get; set; }
}

public class GetInstallmentPortfolioSummaryQueryHandler
    : IRequestHandler<GetInstallmentPortfolioSummaryQuery, GetInstallmentPortfolioSummaryResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetInstallmentPortfolioSummaryQueryHandler> _logger;

    public GetInstallmentPortfolioSummaryQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetInstallmentPortfolioSummaryQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetInstallmentPortfolioSummaryResponse> Handle(GetInstallmentPortfolioSummaryQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetInstallmentPortfolioSummaryAsync(r, r.DataScope, r.Network,
                r.InstallmentBucket, r.PortfolioFlag, r.Urgency, ct);
            return new GetInstallmentPortfolioSummaryResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.InstallmentPortfolioSummaryFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_INSTALLMENT_PORTFOLIO_SUMMARY");
            return new GetInstallmentPortfolioSummaryResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.InstallmentPortfolioSummaryFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

