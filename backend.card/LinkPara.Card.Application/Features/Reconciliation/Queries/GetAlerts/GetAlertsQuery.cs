using LinkPara.Card.Application.Commons.Interfaces.Reconciliation;
using LinkPara.Card.Application.Commons.Models.Reconciliation.Shared;
using LinkPara.Card.Domain.Enums.Reconciliation;
using LinkPara.SharedModels.Pagination;
using MediatR;

namespace LinkPara.Card.Application.Features.Reconciliation.Queries.GetAlerts
{
    public class GetAlertsQuery : SearchQueryParams, IRequest<PaginatedList<Alert>>
    {
        public DateOnly? Date { get; set; }
        public AlertStatus? AlertStatus { get; set; }
    }

    public class GetAlertsQueryHandler : IRequestHandler<GetAlertsQuery, PaginatedList<Alert>>
    {
        private readonly IReconciliationService _service;

        public GetAlertsQueryHandler(IReconciliationService service)
        {
            _service = service;
        }

        public async Task<PaginatedList<Alert>> Handle(GetAlertsQuery request, CancellationToken cancellationToken)
        {
            return await _service.GetAlertsAsync(request, cancellationToken);
        }
    }
}
