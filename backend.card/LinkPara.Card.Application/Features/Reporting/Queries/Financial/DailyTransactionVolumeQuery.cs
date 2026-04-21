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

public class GetDailyTransactionVolumeQuery : SearchQueryParams, IRequest<GetDailyTransactionVolumeResponse>
{
    public DataScope? DataScope { get; set; }
    public string Network { get; set; }
    public string Currency { get; set; }
    public string FinancialType { get; set; }
    public string TxnEffect { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string VolumeFlag { get; set; }
    public string Urgency { get; set; }
}

public class GetDailyTransactionVolumeQueryHandler
    : IRequestHandler<GetDailyTransactionVolumeQuery, GetDailyTransactionVolumeResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetDailyTransactionVolumeQueryHandler> _logger;

    public GetDailyTransactionVolumeQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetDailyTransactionVolumeQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetDailyTransactionVolumeResponse> Handle(GetDailyTransactionVolumeQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetDailyTransactionVolumeAsync(r, r.DataScope, r.Network, r.Currency,
                r.FinancialType, r.TxnEffect, r.DateFrom, r.DateTo, r.VolumeFlag, r.Urgency, ct);
            return new GetDailyTransactionVolumeResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Financial.DailyTransactionVolumeFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_FINANCIAL_DAILY_TRANSACTION_VOLUME");
            return new GetDailyTransactionVolumeResponse
            {
                Message = _localizer.Get("Handler.Reporting.Financial.DailyTransactionVolumeFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

