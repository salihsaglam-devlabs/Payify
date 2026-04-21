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

namespace LinkPara.Card.Application.Features.Reporting.Queries.Operational;

public class GetReconFailureCategorizationQuery : SearchQueryParams, IRequest<GetReconFailureCategorizationResponse>
{
    public DataScope? DataScope { get; set; }
    public string OperationCode { get; set; }
    public string Branch { get; set; }
    public string LikelyRootCause { get; set; }
    public string Urgency { get; set; }
}

public class GetReconFailureCategorizationQueryHandler
    : IRequestHandler<GetReconFailureCategorizationQuery, GetReconFailureCategorizationResponse>
{
    private readonly IReportingService _svc;
    private readonly IReportingErrorMapper _errorMapper;
    private readonly IStringLocalizer _localizer;
    private readonly ILogger<GetReconFailureCategorizationQueryHandler> _logger;

    public GetReconFailureCategorizationQueryHandler(IReportingService svc, IReportingErrorMapper errorMapper,
        Func<LocalizerResource, IStringLocalizer> localizerFactory,
        ILogger<GetReconFailureCategorizationQueryHandler> logger)
    {
        _svc = svc; _errorMapper = errorMapper;
        _localizer = localizerFactory(LocalizerResource.Messages); _logger = logger;
    }

    public async Task<GetReconFailureCategorizationResponse> Handle(
        GetReconFailureCategorizationQuery r, CancellationToken ct)
    {
        try
        {
            var data = await _svc.GetReconFailureCategorizationAsync(r, r.DataScope, r.OperationCode,
                r.Branch, r.LikelyRootCause, r.Urgency, ct);
            return new GetReconFailureCategorizationResponse { Data = data };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer.Get("Handler.Reporting.Operational.ReconFailureCategorizationFailed"));
            var error = _errorMapper.MapException(ex, "HANDLER_REPORTING_OPERATIONAL_RECON_FAILURE_CATEGORIZATION");
            return new GetReconFailureCategorizationResponse
            {
                Message = _localizer.Get("Handler.Reporting.Operational.ReconFailureCategorizationFailed"),
                ErrorCount = 1,
                Errors = new List<ReconciliationErrorDetail> { error }
            };
        }
    }
}

