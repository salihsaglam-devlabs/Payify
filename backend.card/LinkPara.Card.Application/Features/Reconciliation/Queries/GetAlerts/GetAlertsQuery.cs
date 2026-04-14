using LinkPara.Card.Application.Commons.Extensions;
using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Localization;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Contracts.Responses;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.SharedModels.Pagination;
using MediatR;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;

namespace LinkPara.Card.Application.Features.Reconciliation.Queries.GetAlerts
{
    public class GetAlertsQuery : SearchQueryParams, IRequest<GetAlertsResponse>
    {
        public DateOnly? Date { get; set; }
        public AlertStatus? AlertStatus { get; set; }
    }

    public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, GetAlertsResponse>
    {
        private readonly IReconciliationService _service;
        private readonly IReconciliationErrorMapper _errorMapper;
        private readonly IStringLocalizer _localizer;
        private readonly ILogger<GetAlertsQueryHandler> _logger;

        public GetAlertsQueryHandler(
            IReconciliationService service,
            IReconciliationErrorMapper errorMapper,
            Func<LocalizerResource, IStringLocalizer> localizerFactory,
            ILogger<GetAlertsQueryHandler> logger)
        {
            _service = service;
            _errorMapper = errorMapper;
            _localizer = localizerFactory(LocalizerResource.Messages);
            _logger = logger;
        }

        public async Task<GetAlertsResponse> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                return await _service.GetAlertsAsync(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _localizer.Get("Handler.Reconciliation.GetAlertsFailed"));
                var error = _errorMapper.MapException(ex, "HANDLER_GET_ALERTS");
                return new GetAlertsResponse
                {
                    Message = _localizer.Get("Handler.Reconciliation.GetAlertsFailed"),
                    ErrorCount = 1,
                    Errors = new List<ReconciliationErrorDetail> { error }
                };
            }
        }
    }
}
