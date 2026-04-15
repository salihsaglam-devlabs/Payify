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

namespace LinkPara.Card.Application.Features.Reporting.Queries.GetProblemRecords;

public class GetProblemRecordsQuery : SearchQueryParams, IRequest<GetTransactionsResponse>
{
    public ReportingNetwork? Network { get; set; }
    public int? DateFrom { get; set; }
    public int? DateTo { get; set; }
}

public class GetProblemRecordsQueryHandler : IRequestHandler<GetProblemRecordsQuery, GetTransactionsResponse>
{
    private readonly IReportingService _service;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetProblemRecordsQueryHandler> _logger;

    public GetProblemRecordsQueryHandler(
        IReportingService service,
        IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetProblemRecordsQueryHandler> logger)
    {
        _service = service;
        _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages);
        _logger = logger;
    }

    public async Task<GetTransactionsResponse> Handle(GetProblemRecordsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            return await _service.GetProblemRecordsAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.GetProblemsFailed"));
            var error = _errorMapper.MapException(ex, "REPORTING_GET_PROBLEMS");
            return new GetTransactionsResponse
            {
                Message = _localizer.Get("Handler.Reporting.GetProblemsFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}
