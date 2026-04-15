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

namespace LinkPara.Card.Application.Features.Reporting.Queries.GetTransactions;

public class GetTransactionsQuery : SearchQueryParams, IRequest<GetTransactionsResponse>
{
    public int? DateFrom { get; set; }
    public int? DateTo { get; set; }
    public ReportingNetwork? Network { get; set; }
    public ReportingMatchStatus? MatchStatus { get; set; }
    public bool? HasAmountMismatch { get; set; }
    public bool? HasCurrencyMismatch { get; set; }
    public bool? HasDateMismatch { get; set; }
    public bool? HasStatusMismatch { get; set; }
    public ReportingDuplicateStatus? DuplicateStatus { get; set; }
}

public class GetTransactionsQueryHandler : IRequestHandler<GetTransactionsQuery, GetTransactionsResponse>
{
    private readonly IReportingService _service;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetTransactionsQueryHandler> _logger;

    public GetTransactionsQueryHandler(
        IReportingService service,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetTransactionsQueryHandler> logger)
    {
        _service = service;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetTransactionsResponse> Handle(GetTransactionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _service.GetTransactionsAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.GetTransactionsFailed"));
            var error = _errorMapper.MapException(ex, "REPORTING_GET_TRANSACTIONS");
            return new GetTransactionsResponse
            {
                Message = _localizer.Get("Handler.Reporting.GetTransactionsFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}
